#include "StdAfx.h"
#include "PPolyCol.h"
#include "xnamath.h"
#include <cmath>

#include <fstream>



CPPolyCol::CPPolyCol(void)
{
}


CPPolyCol::~CPPolyCol(void)
{
}

HRESULT
CPPolyCol::Create(IDirect3DDevice9 *m_pd3dDevice, CBasicModel **ppModel, int id , void* ppara, void* pmesh, int mcount, void* ptexture, int tcount, CTriangleRenderer* tri)
{
	HRESULT hr = S_OK;

	CPPolyCol *pModel = new CPPolyCol();
    IFCOOM(pModel);
	pModel->myTri=tri;

	pModel->id=id;
    IFC(pModel->Init(m_pd3dDevice, ppara, pmesh, mcount, ptexture, tcount));  //直接创建

    *ppModel = pModel;
    pModel = NULL;



Cleanup:
	delete pModel;
    return hr;


}




HRESULT 
CPPolyCol::Init(IDirect3DDevice9 *m_pd3dDevice, void* ppara, void* pmesh, int mcount, void* ptexture, int tcount)
{


	HRESULT hr = S_OK;


	STRUCT_PolyCol* para=(STRUCT_PolyCol*)ppara;
	this->isReceivePick=para->isReceivePick;
	this->pickFlagId=para->pickFlagId;
	this->deepOrd=para->deepOrd;
	this->rootid=para->rootid;
	this->parentid=para->parentid;
	//this->color=para->color;
	memcpy(&mtrl,&para->material,sizeof(mtrl));
	this->sizex=para->sizex;
	this->sizey=para->sizey;
	//this->height=para->height;
	this->geokey=para->geokey;
	this->aniScale=para->aniScale;
	this->aniRotation=para->aniRotation;

	this->axis=para->axis;
	this->angle=para->angle;


	//初始化本地动画相关数据
	height=0.0001;
	rotateAngle=0;
	this->targetHeight=para->height;
	divHeight=(targetHeight-height);
	starttick= GetTickCount();
	isAni=true;


	if (id != rootid) //非根, 置父对象指针
		pparent=(CPPolyCol*)CPPolyCol::find(rootid,parentid,myTri);
	else
		pparent=nullptr;


	D3DXVECTOR3* pd=(D3DXVECTOR3*)pmesh;
	memcpy(&location,pd,sizeof(location)); //复制

	//设置mesh指针
	{
		auto geoiter=myTri->para.geometries.find(geokey);
		if (geoiter!=myTri->para.geometries.end())
			mesh=geoiter->second;
		else  //若在几何体集合中找不到，转到xmodel中查找
		{
			auto iter=myTri->para.xmodels.find(this->geokey);
			if (iter!=myTri->para.xmodels.end())
				mesh=iter->second->g_pMesh;
		}

	
	}


	// 计算world
	calWorld();


	////设置材质
 //   ZeroMemory( &mtrl, sizeof(D3DMATERIAL9) );
	//D3DXCOLOR c=D3DXCOLOR(color);

	//mtrl.Diffuse.r = mtrl.Ambient.r = c.r;//1.0f;
 //   mtrl.Diffuse.g = mtrl.Ambient.g =c.g;// 0.0f;
 //   mtrl.Diffuse.b = mtrl.Ambient.b =c.b;// 0.0f;
 //   mtrl.Diffuse.a = mtrl.Ambient.a =c.a;// 1.0f;


Cleanup:
    return hr;

}

void
CPPolyCol::calWorld()
{
	D3DXMATRIXA16 tmp;
	D3DXMatrixIdentity(&world);

	//d3dcreate生成的，进行先期位移
	D3DXMatrixTranslation(&tmp,0,0,0.5);
	world*=tmp;

	//旋转
	D3DXMatrixRotationZ(&tmp,rotateAngle);
	world*=tmp;

	//缩放
	D3DXMatrixScaling(&tmp,sizex,sizey,height);
	world*=tmp;


	//位移到指定经纬度高
	if (pparent!=nullptr) //有父对象
		distanceGround = pparent->distanceGround + pparent->height;
	else
		distanceGround=0;
	


	if (myTri->earthpara.SceneMode==地球)
	{
		D3DXVECTOR3 locationD;
		D3DXVec3Scale(&locationD,&location,(1+distanceGround/D3DXVec3Length(&location)));
		D3DXVECTOR3 location2=D3DXVECTOR3(0,0,myTri->para.Radius+distanceGround);
		D3DXMatrixTranslation(&tmp,0,0,myTri->para.Radius+distanceGround);
		world *= tmp;
		tmp=CHelper::getMatrixP2P(location2,locationD);
	}
	else
	{
		D3DXMatrixTranslation(&tmp, location.x,location.y, distanceGround);
	}
	world *= tmp;

	//计算子项
	for(auto iter=submodels.begin();iter!=submodels.end();iter++)
	{
		((iter->second))->calWorld();
	}

}



HRESULT
CPPolyCol::Render(IDirect3DDevice9 *m_pd3dDevice)
{
	HRESULT hr = S_OK;



	

	if (isAni) //本地动画
	{
		UINT  iTime  = GetTickCount()- starttick;
		float process = (float)iTime / 1000;
		if (process>1)
		{
			height=targetHeight;
			isAni=false;
		}
		else
			height=targetHeight-(1-process)*divHeight;
		calWorld();
	}
	if (aniRotation.isDoAni)  //若旋转动画
	{
		float progress=(float)((GetTickCount()-aniRotation.startTick)%aniRotation.duration)/aniRotation.duration;
		aniRotation.doneCount=(GetTickCount()-aniRotation.startTick)/aniRotation.duration;
		if (aniRotation.doCount==0 || aniRotation.doneCount<aniRotation.doCount)
		{
			rotateAngle=progress*D3DX_PI*2;
			calWorld();
		}
		else
		{
			aniRotation.doneCount=0;
			aniRotation.isDoAni=false;
		}
	}
	IFC(m_pd3dDevice->SetTransform(D3DTS_WORLD, &world));

	

    m_pd3dDevice->SetMaterial( &mtrl );
	m_pd3dDevice->SetRenderState( D3DRS_SPECULARENABLE, mtrl.Power>0 );

		



	{
		auto geoiter=myTri->para.geometries.find(geokey);
		if (geoiter!=myTri->para.geometries.end())
			geoiter->second->DrawSubset(0);
		else  //若在几何体集合中找不到，转到xmodel中查找
		{
			auto iter=myTri->para.xmodels.find(this->geokey);
			if (iter!=myTri->para.xmodels.end())
				iter->second->Render();
		}
	}



	for (auto iter=submodels.begin();iter!=submodels.end();iter++)
		((CBasicModel*)(iter->second))->Render(m_pd3dDevice);



	

Cleanup:

    return hr;

}


void
CPPolyCol::SetEffect(IDirect3DDevice9 *m_pd3dDevice,CTriangleRenderer* myTri)
{
	//设置半透混合
	m_pd3dDevice->SetRenderState(D3DRS_ALPHABLENDENABLE,true);
	m_pd3dDevice->SetRenderState(D3DRS_SRCBLEND,D3DBLEND_SRCALPHA);
	m_pd3dDevice->SetRenderState(D3DRS_DESTBLEND,D3DBLEND_INVSRCALPHA);
//
//
//////设置材质
//    D3DMATERIAL9 mtrl;
//    ZeroMemory( &mtrl, sizeof(D3DMATERIAL9) );
//    mtrl.Diffuse.r = mtrl.Ambient.r = 1.0f;
//    mtrl.Diffuse.g = mtrl.Ambient.g = 1.0f;
//    mtrl.Diffuse.b = mtrl.Ambient.b = 1.0f;
//    mtrl.Diffuse.a = mtrl.Ambient.a = 1.0f;
//    m_pd3dDevice->SetMaterial( &mtrl );

 //   //设置灯光
 //   D3DXVECTOR3 vecDir;
 //   D3DLIGHT9 light;
 //   ZeroMemory( &light, sizeof(D3DLIGHT9) );
 //   light.Type       = D3DLIGHT_DIRECTIONAL;
 //   light.Diffuse.r  = 0.5f;
 //   light.Diffuse.g  = 0.5f;
 //   light.Diffuse.b  = 0.5f;
	//
	////测试，计算个光照方向
	//vecDir = D3DXVECTOR3(-0.0f, -0.0f, -1.0f);
	//D3DXMATRIXA16 tmp;
	//D3DXMATRIXA16 matrix;	
	//D3DXVECTOR3 location=D3DXVECTOR3(1925.469,4152.846,4441.517);
	//D3DXVECTOR3 location2=D3DXVECTOR3(0,0,myTri->para.Radius);
	//D3DXMatrixTranslation(&matrix,0,0,myTri->para.Radius);
	//tmp=CHelper::getMatrixP2P(location2,location);
	//matrix*=tmp;
	//D3DXVec3TransformCoord(&vecDir,&vecDir,&matrix);


 //   D3DXVec3Normalize( (D3DXVECTOR3*)&light.Direction, &vecDir );
 //   light.Range       = 1000.0f;
 //   m_pd3dDevice->SetLight( 0, &light );
 //   m_pd3dDevice->LightEnable( 0, TRUE );
 //   m_pd3dDevice->SetRenderState( D3DRS_LIGHTING, TRUE );
	
    //设置环境光
    //m_pd3dDevice->SetRenderState( D3DRS_AMBIENT, 0x00111111);


}


void
CPPolyCol::ChangeProperty(EPropertyType propertytype,void* para,int count)
{
	if (propertytype==高度)
	{
		//memcpy(&height,(float*)para,sizeof(height));
		//calWorld();

		//本地动画
		memcpy(&targetHeight,(float*)para,sizeof(targetHeight));
		divHeight=(targetHeight-height);
		starttick= GetTickCount();
		isAni=true;
	}
	else if (propertytype=材质)
	{
		//memcpy(&color,(float*)para,sizeof(color));
		//changeColor(color);
		memcpy(&mtrl,(D3DMATERIAL9*)para,sizeof(mtrl));
	}
	else if (propertytype==动画)
	{
		STRUCT_Ani* lpara=(STRUCT_Ani*)para;
		if (lpara->aniType==旋转)
		{
			memcpy(&aniRotation,para,sizeof(aniRotation));
			aniRotation.startTick=GetTickCount();
		}
	}
}






