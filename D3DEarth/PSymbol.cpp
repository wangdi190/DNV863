#include "StdAfx.h"
#include "PSymbol.h"


CPSymbol::CPSymbol(void)
{
	progress=1;
}


CPSymbol::~CPSymbol(void)
{
}

HRESULT
CPSymbol::Create(IDirect3DDevice9 *m_pd3dDevice, CBasicModel **ppModel, int id , void* ppara, void* pmesh, int mcount, void* ptexture, int tcount, CTriangleRenderer* tri)
{
	HRESULT hr = S_OK;

	CPSymbol *pModel = new CPSymbol();
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
CPSymbol::Init(IDirect3DDevice9 *m_pd3dDevice, void* ppara, void* pmesh, int mcount, void* ptexture, int tcount)
{
	HRESULT hr = S_OK;

	STRUCT_Symbol* para=(STRUCT_Symbol*)ppara;
	this->isReceivePick=para->isReceivePick;
	this->pickFlagId=para->pickFlagId;
	this->deepOrd=para->deepOrd;
	this->rootid=para->rootid;
	this->parentid=para->parentid;

	this->isH=para->isH;
	this->scaleX=para->scaleX;
	this->scaleY=para->scaleY;
	this->scaleZ=para->scaleZ;
	saveX=scaleX;
	saveY=scaleY;
	saveZ=scaleZ;
	this->texturekey=para->texturekey;
	this->aniTwinkle=para->aniTwinkle;
	this->aniShow=para->aniShow;
	this->aniScale=para->aniScale;
	//this->color=para->color;
	memcpy(&mtrl,&para->material,sizeof(mtrl));
	savealpha=mtrl.Ambient.a;
	this->isUseColor=para->isUseColor;
	if (aniTwinkle.isDoAni) aniTwinkle.startTick=GetTickCount();
	if (aniShow.isDoAni) aniShow.startTick=GetTickCount();
	if (aniScale.isDoAni) aniScale.startTick=GetTickCount();

	this->isUseXModel=para->isUseXModel;
	this->XMKey=para->XMKey;
	this->XMScaleAddition=para->XMScaleAddition;

	this->axis=para->axis;
	this->angle=para->angle;

	D3DXVECTOR3* pd=(D3DXVECTOR3*)pmesh;
	memcpy(&location,pd,sizeof(location)); //复制

	if (id != rootid) //非根, 置父对象指针
		pparent=(CPPolyCol*)CPPolyCol::find(rootid,parentid,myTri);
	else
		pparent=nullptr;



	CUSTOMVERTEX_T* vertices(nullptr);
	vertices=new CUSTOMVERTEX_T[4];
	if (isH)//水平
	{
		vertices[0].position= D3DXVECTOR3(1,1,0); vertices[0].normal=D3DXVECTOR3(0,0,1); vertices[0].u=1; vertices[0].v=0;
		vertices[1].position= D3DXVECTOR3(-1,1,0); vertices[1].normal=D3DXVECTOR3(0,0,1); vertices[1].u=0; vertices[1].v=0;
		vertices[2].position= D3DXVECTOR3(1,-1,0); vertices[2].normal=D3DXVECTOR3(0,0,1); vertices[2].u=1; vertices[2].v=1;
		vertices[3].position= D3DXVECTOR3(-1,-1,0); vertices[3].normal=D3DXVECTOR3(0,0,1); vertices[3].u=0; vertices[3].v=1;
	}
	else //垂直地面
	{
		vertices[0].position= D3DXVECTOR3(1,0,2); vertices[0].normal=D3DXVECTOR3(0,1,0); vertices[0].u=1; vertices[0].v=0;
		vertices[1].position= D3DXVECTOR3(-1,0,2); vertices[1].normal=D3DXVECTOR3(0,1,0); vertices[1].u=0; vertices[1].v=0;
		vertices[2].position= D3DXVECTOR3(1,0,0); vertices[2].normal=D3DXVECTOR3(0,1,0); vertices[2].u=1; vertices[2].v=1;
		vertices[3].position= D3DXVECTOR3(-1,0,0); vertices[3].normal=D3DXVECTOR3(0,1,0); vertices[3].u=0; vertices[3].v=1;
	}
	NumVertices=4; //顶点数
	PrimCount=2; //三角数


	// 建立mesh
	D3DXCreateMeshFVF(PrimCount,NumVertices,D3DXMESH_DYNAMIC, D3DFVF_CUSTOMVERTEX_T, m_pd3dDevice, &mesh);

	CUSTOMVERTEX_T* v = 0;
	mesh->LockVertexBuffer(0, (void**)&v);
	for (int i=0;i<NumVertices;i++)
		v[i]=vertices[i];
	mesh->UnlockVertexBuffer();


	WORD* idx = 0;
	mesh->LockIndexBuffer(0, (void**)&idx);
	idx[0]=0;
	idx[1]=1;
	idx[2]=2;

	idx[3]=2;
	idx[4]=1;
	idx[5]=3;
	mesh->UnlockIndexBuffer();





	////===== 顶点缓冲	
	//IFC(m_pd3dDevice->CreateVertexBuffer(NumVertices*sizeof(CUSTOMVERTEX_T), 0, D3DFVF_CUSTOMVERTEX_T, D3DPOOL_DEFAULT, &m_pd3dVB, NULL)); 
	//void *pVertices;
	//IFC(m_pd3dVB->Lock(0, NumVertices*sizeof(CUSTOMVERTEX_T), &pVertices, 0));
	//memcpy(pVertices, vertices, NumVertices*sizeof(CUSTOMVERTEX_T));
	//m_pd3dVB->Unlock();
	//===== 纹理
	if (texturekey==0)  //传入纹理
	{
        D3DXCreateTextureFromFileInMemory(myTri->m_pd3dDevice,ptexture,tcount*sizeof(byte),&g_pTexture);
	}
	else  //公用纹理库中取纹理
	{
		auto iter=myTri->para.textures.find(texturekey);
		if (iter!=myTri->para.textures.end())
		{
			isDicTexture=true;
			g_pTexture=iter->second;
		}
	}

	//IFC(D3DXCreateTextureFromFile(m_pd3dDevice,L"2.jpg",&g_pTexture));

	////设置材质
 //   ZeroMemory( &mtrl, sizeof(D3DMATERIAL9) );
	//D3DXCOLOR c=D3DXCOLOR(color);

	//mtrl.Diffuse.r = mtrl.Ambient.r =c.r;//1.0f;
 //   mtrl.Diffuse.g = mtrl.Ambient.g =c.g;// 0.0f;
 //   mtrl.Diffuse.b = mtrl.Ambient.b =c.b;// 0.0f;
 //   mtrl.Diffuse.a = mtrl.Ambient.a =c.a;// 1.0f;

	//mtrl.Diffuse.r =0.8;// c.r*255;
	//mtrl.Ambient.r =c.r*255*0;//1.0f;
 //   mtrl.Diffuse.g =0.8;// c.r*255;
	//mtrl.Ambient.g =c.g*255*0;// 0.0f;
 //   mtrl.Diffuse.b =0.8;// c.r*255;
	//mtrl.Ambient.b =c.b*255*0;// 0.0f;
 //   mtrl.Diffuse.a = mtrl.Ambient.a =255;//c.a;// 1.0f;


	//====== 计算world

	calWorld();


Cleanup:
	delete vertices;
    return hr;

}

HRESULT
CPSymbol::Render(IDirect3DDevice9 *m_pd3dDevice)
{
	
	HRESULT hr = S_OK;



		float alpha=savealpha;
		if (aniTwinkle.isDoAni) //闪烁动画
		{
			alpha=(float)((GetTickCount()-aniTwinkle.startTick)%aniTwinkle.duration)/aniTwinkle.duration;
			aniTwinkle.doneCount=(GetTickCount()-aniTwinkle.startTick)/aniTwinkle.duration;
			if (aniTwinkle.doCount==0 || aniTwinkle.doneCount<aniTwinkle.doCount)
			{
				if (aniTwinkle.isReverse && aniTwinkle.doneCount%2==1)
					alpha=1-alpha;
				alpha+=0.1;
			}
			else
			{
				aniTwinkle.doneCount=0;
				aniTwinkle.isDoAni=false;
			}
		}

		if (aniShow.isDoAni) //渐变显示动画
		{
			//aniShow.doCount=0;aniShow.isReverse=true;
			alpha=(float)((GetTickCount()-aniShow.startTick)%aniShow.duration)/aniShow.duration;
			aniShow.doneCount=(GetTickCount()-aniShow.startTick)/aniShow.duration;
			if (aniShow.doCount==0 || aniShow.doneCount<aniShow.doCount)
			{
				if (aniShow.isReverse && aniShow.doneCount%2==1)
					alpha=1-alpha;
			}
			else
			{
				aniShow.doneCount=0;
				alpha=1;
				aniShow.isDoAni=false;
			}
		}

		if (aniScale.isDoAni) //缩放动画
		{
			progress=(float)((GetTickCount()-aniScale.startTick)%aniScale.duration)/aniScale.duration;
			aniScale.doneCount=(GetTickCount()-aniScale.startTick)/aniScale.duration;
			float addscale;
			if (aniScale.doCount==0 || aniScale.doneCount<aniScale.doCount)
			{
				addscale=(0.5-abs(0.5-progress))*0.5; //控制缩放幅度
				if (aniScale.doneCount%2==0)
					addscale=1+addscale;
				else
					addscale=1-addscale;
				scaleX=saveX*addscale;
				scaleY=saveY*addscale;
				calWorld();
			}
			else
			{
				aniScale.doneCount=0;
				alpha=1;
				aniScale.isDoAni=false;
				progress=1;
				scaleX=saveX;
				scaleY=saveY;
			}
		}





		mtrl.Diffuse.a = mtrl.Ambient.a=mtrl.Emissive.a = alpha;
		myTri->m_pd3dDevice->SetMaterial( &mtrl );

		m_pd3dDevice->SetRenderState( D3DRS_LIGHTING, TRUE );

		m_pd3dDevice->SetRenderState(D3DRS_ZWRITEENABLE,true);


		//m_pd3dDevice->SetRenderState(D3DRS_ALPHABLENDENABLE,true);
		//m_pd3dDevice->SetRenderState(D3DRS_SRCBLEND,D3DBLEND_SRCALPHA);
		//m_pd3dDevice->SetRenderState(D3DRS_DESTBLEND,D3DBLEND_INVSRCALPHA);

		m_pd3dDevice->SetSamplerState(0, D3DSAMP_MIPFILTER, D3DTEXF_LINEAR);
		IFC(m_pd3dDevice->SetTransform(D3DTS_WORLD, &world));



			if (this->isUseXModel)
	{

		IFC(m_pd3dDevice->SetTransform(D3DTS_WORLD, &world));
		
		auto iter=myTri->para.xmodels.find(this->XMKey);
		if (iter!=myTri->para.xmodels.end())
			iter->second->Render();
		else  //若在xmodel集合中找不到，转到几何体查找
		{
			m_pd3dDevice->SetMaterial( &mtrl );
			auto geoiter=myTri->para.geometries.find(this->XMKey);
			if (geoiter!=myTri->para.geometries.end())
				geoiter->second->DrawSubset(0);
		}
	}
	else
	{

		//一次渲染：新图元渐现
		m_pd3dDevice->SetTextureStageState(0,D3DTSS_COLORARG1,D3DTA_DIFFUSE);
		m_pd3dDevice->SetTextureStageState(0,D3DTSS_COLORARG2,D3DTA_TEXTURE);
		if (isUseColor)
			m_pd3dDevice->SetTextureStageState(0,D3DTSS_COLOROP,D3DTOP_SELECTARG1);  //使用材质色
		else
			m_pd3dDevice->SetTextureStageState(0,D3DTSS_COLOROP,D3DTOP_SELECTARG2);  //使用纹理本身



		m_pd3dDevice->SetTextureStageState(0,D3DTSS_ALPHAARG1,D3DTA_DIFFUSE);
		m_pd3dDevice->SetTextureStageState(0,D3DTSS_ALPHAARG2,D3DTA_TEXTURE);
		m_pd3dDevice->SetTextureStageState(0,D3DTSS_ALPHAOP,D3DTOP_MODULATE);
		IFC(m_pd3dDevice->SetTexture(0,g_pTexture));





		mesh->DrawSubset(0);

		if (g_pTexture2!=NULL && aniShow.isDoAni)  //二次渲染，旧图元渐隐
		{
			mtrl.Diffuse.a = mtrl.Ambient.a = 1-alpha;//0.5f;
			m_pd3dDevice->SetMaterial( &mtrl );
			IFC(m_pd3dDevice->SetTexture(0,g_pTexture2));
			m_pd3dDevice->SetTextureStageState(0,D3DTSS_ALPHAARG1,D3DTA_DIFFUSE);
			m_pd3dDevice->SetTextureStageState(0,D3DTSS_ALPHAARG2,D3DTA_TEXTURE);
			m_pd3dDevice->SetTextureStageState(0,D3DTSS_ALPHAOP,D3DTOP_MODULATE);
			mesh->DrawSubset(0);
		}
	}
	
	
	IFC(m_pd3dDevice->SetTexture(0,nullptr));
	D3DXMATRIXA16 tmp;
	D3DXMatrixIdentity(&tmp);
	IFC(m_pd3dDevice->SetTransform(D3DTS_WORLD, &tmp));

	m_pd3dDevice->SetSamplerState(0, D3DSAMP_MIPFILTER, D3DTEXF_NONE);


Cleanup:

    return hr;

}

void
CPSymbol::SetEffect(IDirect3DDevice9 *m_pd3dDevice,CTriangleRenderer* myTri)
{
	

}


void
CPSymbol::calWorld()
{
	//位移到指定经纬度高
	float distanceGround;
	if (pparent!=nullptr) //有父对象
		distanceGround = pparent->distanceGround + pparent->height;
	else
		distanceGround=myTri->para.TextHeight;

	D3DXMATRIXA16 tmp;
	if (this->isUseXModel)
	{
		D3DXMatrixScaling(&world,scaleX*XMScaleAddition,scaleY*XMScaleAddition,scaleZ*XMScaleAddition);
		//D3DXMatrixRotationX(&tmp,D3DX_PI/2);

		auto iter=myTri->para.xmodels.find(this->XMKey);
		if (iter!=myTri->para.xmodels.end())
		{
			D3DXMatrixRotationAxis(&tmp, &iter->second->axis, iter->second->angle); 
			world*=tmp;
		}
	}
	else
		D3DXMatrixScaling(&world,scaleX,scaleY,scaleZ);

	D3DXMatrixRotationAxis(&tmp, &axis, angle); //自身初始旋转
	world*=tmp;


	if (myTri->earthpara.SceneMode==地球)
	{
		D3DXVECTOR3 location2=D3DXVECTOR3(0,0,myTri->para.Radius+distanceGround);
		D3DXMatrixTranslation(&tmp,0,0,myTri->para.Radius+distanceGround);
		world*=tmp;
		tmp=CHelper::getMatrixP2P(location2,location);
	}
	else
	{
		D3DXMatrixTranslation(&tmp, location.x,location.y, location.z);
	}

	world*=tmp;

		
}


void
CPSymbol::ChangeProperty(EModelType modeltype, EPropertyType propertytype,void* para,int count)
{
	if (modeltype==图元)
	{
		if (propertytype==内容)
		{
			memcpy(&texturekey,(int*)para,sizeof(texturekey));
			auto iter=myTri->para.textures.find(texturekey);
			if (iter!=myTri->para.textures.end())
			{
				g_pTexture2=g_pTexture;
				isDicTexture2=isDicTexture;
				g_pTexture=iter->second;	
				isDicTexture=true;
			}
			aniShow.isDoAni=true;
			aniShow.startTick=GetTickCount();
		}
		else if (propertytype==类型)  //是否使用xmodel
		{
            memcpy(&isUseXModel,para,sizeof(isUseXModel));
			calWorld();
		}
		else if (propertytype==纹理)
		{
			g_pTexture2=g_pTexture;
			isDicTexture2=isDicTexture;
			D3DXCreateTextureFromFileInMemory(myTri->m_pd3dDevice,para,count*sizeof(byte),&g_pTexture);
			isDicTexture=false;
			aniShow.isDoAni=true;
			aniShow.startTick=GetTickCount();
		}
		else if (propertytype==进度)
		{
			memcpy(&progress,(float*)para,sizeof(progress));
			scaleX=saveX*progress;
			scaleY=saveY*progress;
			calWorld();
	
		}
		else if (propertytype==长度)
		{
			memcpy(&saveX,(float*)para,sizeof(saveX));
			scaleX=saveX*progress;
			calWorld();
		}
		else if (propertytype==宽度)
		{
			memcpy(&saveY,(float*)para,sizeof(saveY));
			scaleY=saveY*progress;
			calWorld();
		}
		else if (propertytype==高度)
		{
			memcpy(&saveZ,(float*)para,sizeof(saveZ));
			scaleZ=saveZ*progress;
			calWorld();
		}
		else if (propertytype==材质)
		{
			//memcpy(&color,(DWORD*)para,sizeof(color));
			//D3DXCOLOR c=D3DXCOLOR(color);
			//mtrl.Diffuse.r = mtrl.Ambient.r = c.r;//1.0f;
			//mtrl.Diffuse.g = mtrl.Ambient.g =c.g;// 0.0f;
			//mtrl.Diffuse.b = mtrl.Ambient.b =c.b;// 0.0f;
			//mtrl.Diffuse.a = mtrl.Ambient.a =c.a;// 1.0f;
			memcpy(&mtrl,(D3DMATERIAL9*)para,sizeof(mtrl));
		}
		else if(propertytype==位置)
		{
			memcpy(&location,(D3DXVECTOR3*)para,sizeof(location)); 
			calWorld();
		}
		else if (propertytype==角度)
		{
			memcpy(&angle,(float*)para,sizeof(angle));
			calWorld();
		}
		else if(propertytype==动画)
		{
			STRUCT_Ani* lpara=(STRUCT_Ani*)para;
			if (lpara->aniType==闪烁)
			{
				memcpy(&aniTwinkle,para,sizeof(aniTwinkle));
				aniTwinkle.startTick=GetTickCount();
			}
			else if (lpara->aniType==渐变)
			{
				memcpy(&aniShow,para,sizeof(aniShow));
				aniShow.startTick=GetTickCount();
			}
			else if (lpara->aniType==缩放)
			{
				memcpy(&aniScale,para,sizeof(aniScale));
				if (aniScale.isDoAni)
				{
					aniScale.startTick=GetTickCount();
					saveX=scaleX;
					saveY=scaleY;
				}
				else
				{
					scaleX=saveX;
					scaleY=saveY;
					calWorld();
				}
			}
		}
	
	}
	


}


//------------------------------------------------
//拾取模型，返回id
//------------------------------------------------
int
CPSymbol::Pick_Model(IDirect3DDevice9* pd3dDevice, POINT ptCursor)
{
	int result=0;
	if (isUseXModel)
	{
		auto iter=myTri->para.xmodels.find(this->XMKey);
		if (iter!=myTri->para.xmodels.end())
			result=Pick_ModelMesh(pd3dDevice, ptCursor, iter->second->g_pMesh);
		else  //若在xmodel集合中找不到，转到几何体查找
		{
			auto geoiter=myTri->para.geometries.find(this->XMKey);
			if (geoiter!=myTri->para.geometries.end())
				result=Pick_ModelMesh(pd3dDevice, ptCursor, geoiter->second);
			else
				return 0;
		}
	}
	else
		result=Pick_ModelMesh(pd3dDevice, ptCursor, mesh);

	if( result !=0 )
	{
		return id;
	}
	else
	{
		for (auto subiter=submodels.begin();subiter!=submodels.end();subiter++)
		{
			result=subiter->second->Pick_Model(pd3dDevice,ptCursor);
			if (result!=0)
				return result;
		}
	}

	


	return result;

}
