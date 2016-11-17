#include "StdAfx.h"
#include "PCustom.h"


CPCustom::CPCustom(void)
{
}


CPCustom::~CPCustom(void)
{
}

HRESULT
CPCustom::Create(IDirect3DDevice9 *m_pd3dDevice, CBasicModel **ppModel, int id , void* ppara,void* plocation, void* pvertices, void* pnormal, int vcount, void* pindex, int icount, void* puv, int uvcount, void* ptexture, CTriangleRenderer* tri)
{
	HRESULT hr = S_OK;

	CPCustom *pModel = new CPCustom();
    IFCOOM(pModel);
	pModel->myTri=tri;

	pModel->id=id;
    IFC(pModel->Init(m_pd3dDevice, ppara, plocation, pvertices, pnormal, vcount, pindex, icount, puv, uvcount, ptexture));  //直接创建

    *ppModel = pModel;
    pModel = NULL;

Cleanup:
    delete pModel;

    return hr;
}
HRESULT
CPCustom::Init(IDirect3DDevice9 *m_pd3dDevice, void* ppara,void* plocation , void* pvertices, void* pnormal, int vcount, void* pindex, int icount, void* puv,int uvcount, void* ptexture)
{
	HRESULT hr = S_OK;

	STRUCT_Custom* para=(STRUCT_Custom*)ppara;
	this->isReceivePick=para->isReceivePick;
	this->pickFlagId=para->pickFlagId;
	this->deepOrd=para->deepOrd;
	this->rootid=para->rootid;
	this->parentid=para->parentid;
	this->texturekey=para->texturekey;
	this->scaleX=para->scaleX;
	this->scaleY=para->scaleY;
	this->scaleZ=para->scaleZ;
	saveX=scaleX;
	saveY=scaleY;
	saveZ=scaleZ;
	//this->color=para->color;
	memcpy(&mtrl,&para->material,sizeof(mtrl));

	this->axis=para->axis;
	this->angle=para->angle;
	this->drawMode=para->drawMode;

	D3DXVECTOR3* pd=(D3DXVECTOR3*)plocation;
	memcpy(&location,pd,sizeof(location)); //复制位置

	//if (id != rootid) //非根, 置父对象指针
	//	pparent=(CPPolyCol*)CPPolyCol::find(rootid,parentid,myTri);
	//else
	//	pparent=nullptr;

		//===== 纹理
	if (texturekey==0)  //传入纹理
	{
		if (ptexture!=nullptr)
		{
			wstring file((WCHAR*)ptexture);
			D3DXCreateTextureFromFile(CGlobal::pCurRender->m_pd3dDevice,file.c_str(),&g_pTexture);
		}
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


	buildMesh( pvertices, pnormal, vcount, pindex, icount, puv, uvcount);



	
	//////设置材质
 //   ZeroMemory( &mtrl, sizeof(D3DMATERIAL9) );
	//D3DXCOLOR c=D3DXCOLOR(color);

	//mtrl.Diffuse.r = mtrl.Ambient.r = c.r;//1.0f;
 //   mtrl.Diffuse.g = mtrl.Ambient.g =c.g;// 0.0f;
 //   mtrl.Diffuse.b = mtrl.Ambient.b =c.b;// 0.0f;
 //   mtrl.Diffuse.a = mtrl.Ambient.a =c.a;// 1.0f;


	//====== 计算world

	calWorld();


Cleanup:
    return hr;

}


//--------------------------------------------------------------------
// 根据点集，创建mesh
//--------------------------------------------------------------------
void 
CPCustom::buildMesh(void* pvertices, void* pnormal, int vcount, void* pindex, int icount, void* puv,int uvcount)
{
	D3DXVECTOR3* pv=(D3DXVECTOR3*)pvertices;
	D3DXVECTOR3* pn;
	if (pnormal!=nullptr)
		pn=(D3DXVECTOR3*)pnormal;
	D3DXVECTOR2* uv;
	if (puv!=nullptr)
		uv=(D3DXVECTOR2*)puv;

	CUSTOMVERTEX_T* vertices(nullptr);
	vertices=new CUSTOMVERTEX_T[vcount];

	for (int i=0; i<vcount; i++)
	{
		vertices[i].position=D3DXVECTOR3(pv[i].x,pv[i].y,pv[i].z);
		if (pnormal!=nullptr)
		{
			vertices[i].normal=D3DXVECTOR3(pn[i].x,pn[i].y,pn[i].z);
			D3DXVec3Normalize(&vertices[i].normal,&vertices[i].normal);
		}
		if (puv!=nullptr)
		{
			vertices[i].u=uv[i].x;
			vertices[i].v=uv[i].y;
		}
	}

	WORD* pi=(WORD*)pindex;
	WORD* idxes(nullptr);
	idxes=new WORD[icount];
	memcpy(idxes,pi,sizeof(pi[0])*icount); //复制索引 

	NumVertices=vcount; //顶点数
	PrimCount=icount/3; //三角数


	// 建立mesh
	if (mesh!=NULL)
		mesh->Release();

	D3DXCreateMeshFVF(PrimCount,NumVertices,D3DXMESH_DYNAMIC, D3DFVF_CUSTOMVERTEX_T,myTri->m_pd3dDevice, &mesh);
	
	CUSTOMVERTEX_T* v = 0;
	mesh->LockVertexBuffer(0, (void**)&v);
	for (int i=0;i<NumVertices;i++)
		v[i]=vertices[i];
	mesh->UnlockVertexBuffer();

 
	WORD* idx = 0;
	mesh->LockIndexBuffer(0, (void**)&idx);
	for (int i=0;i<icount;i++)
	{
		idx[i]=idxes[i];
	}
	mesh->UnlockIndexBuffer();
	//优化
	vector<DWORD> adjacencyBuffer(mesh->GetNumFaces() * 3);
	mesh->GenerateAdjacency(0.0f, &adjacencyBuffer[0]);
	mesh->OptimizeInplace(D3DXMESHOPT_ATTRSORT|D3DXMESHOPT_COMPACT|D3DXMESHOPT_VERTEXCACHE, &adjacencyBuffer[0],0, 0, 0);








	////===== 顶点缓冲	
	//myTri->m_pd3dDevice->CreateVertexBuffer(NumVertices*sizeof(CUSTOMVERTEX_T), 0, D3DFVF_CUSTOMVERTEX_T, D3DPOOL_DEFAULT, &m_pd3dVB, NULL); 
	//void *pVertices;
	//m_pd3dVB->Lock(0, NumVertices*sizeof(CUSTOMVERTEX_T), &pVertices, 0);
	//memcpy(pVertices, vertices, NumVertices*sizeof(CUSTOMVERTEX_T));
	//m_pd3dVB->Unlock();
	////===== 索引缓冲
	//myTri->m_pd3dDevice->CreateIndexBuffer(PrimCount*3*sizeof(WORD), 0, D3DFMT_INDEX16 , D3DPOOL_DEFAULT, &m_pd3dIB, NULL); 
	//void *pIndexes;
	//m_pd3dIB->Lock(0, PrimCount*3*sizeof(WORD), &pIndexes, 0);
	//memcpy(pIndexes, idxes, PrimCount*3*sizeof(WORD));
	//m_pd3dIB->Unlock();






	Cleanup:
	delete vertices;
	delete idxes;
}


HRESULT
CPCustom::Render(IDirect3DDevice9 *m_pd3dDevice)
{

	HRESULT hr = S_OK;

	m_pd3dDevice->SetRenderState( D3DRS_LIGHTING, TRUE );
	m_pd3dDevice->SetRenderState(D3DRS_ALPHABLENDENABLE,true);
	m_pd3dDevice->SetRenderState(D3DRS_SRCBLEND,D3DBLEND_SRCALPHA);
	m_pd3dDevice->SetRenderState(D3DRS_DESTBLEND,D3DBLEND_INVSRCALPHA);
	m_pd3dDevice->SetRenderState( D3DRS_CULLMODE, D3DCULL_NONE );
	IFC(m_pd3dDevice->SetTransform(D3DTS_WORLD, &world));
	if (drawMode==纯色模式)
	{
		IFC(m_pd3dDevice->SetTexture(0,nullptr));
		m_pd3dDevice->SetRenderState(D3DRS_ZWRITEENABLE,mtrl.Diffuse.a==1); //zh深度设置, 若为半透，需设为false
		myTri->m_pd3dDevice->SetMaterial( &mtrl );
	}
	else if (drawMode==纹理模式)
	{
		m_pd3dDevice->SetRenderState(D3DRS_ZWRITEENABLE,true);
		IFC(m_pd3dDevice->SetTexture(0,g_pTexture));
	}
	else if (drawMode==线框模式)
	{
		IFC(m_pd3dDevice->SetTexture(0,nullptr));
		myTri->m_pd3dDevice->SetMaterial( &mtrl );
		m_pd3dDevice->SetRenderState(D3DRS_FILLMODE,D3DFILL_WIREFRAME);

	}

	mesh->DrawSubset(0);


	////m_pd3dDevice->SetRenderState(D3DRS_ZWRITEENABLE,mtrl.Diffuse.a==1); //zh深度设置, 若为半透，需设为false
	//m_pd3dDevice->SetRenderState(D3DRS_ZWRITEENABLE,true);

	//CGlobal::pCurRender->m_pd3dDevice->SetRenderState( D3DRS_CULLMODE, D3DCULL_NONE );

	//{
	//	myTri->m_pd3dDevice->SetMaterial( &mtrl );

	//	m_pd3dDevice->SetRenderState( D3DRS_LIGHTING, TRUE );
	//	
	//	
	//	m_pd3dDevice->SetRenderState( D3DRS_AMBIENT, 0x80ffffff);




	//	m_pd3dDevice->SetRenderState(D3DRS_ALPHABLENDENABLE,true);
	//	m_pd3dDevice->SetRenderState(D3DRS_SRCBLEND,D3DBLEND_SRCALPHA);
	//	m_pd3dDevice->SetRenderState(D3DRS_DESTBLEND,D3DBLEND_INVSRCALPHA);

	//	IFC(m_pd3dDevice->SetTransform(D3DTS_WORLD, &world));


 //       IFC(m_pd3dDevice->SetTexture(0,g_pTexture));


	//	//m_pd3dDevice->SetTextureStageState(0,D3DTSS_COLORARG1,D3DTA_DIFFUSE);
	//	//m_pd3dDevice->SetTextureStageState(0,D3DTSS_COLORARG2,D3DTA_TEXTURE);
	//	//m_pd3dDevice->SetTextureStageState(0,D3DTSS_COLOROP,D3DTOP_SELECTARG2);  //使用材质色

	//	
	//	
	//	//IFC(m_pd3dDevice->SetStreamSource(0, m_pd3dVB, 0, sizeof(CUSTOMVERTEX_T)));
	//	//IFC(m_pd3dDevice->SetFVF(D3DFVF_CUSTOMVERTEX_T));
	//	//IFC(m_pd3dDevice->SetIndices(m_pd3dIB));  
	//	//IFC(m_pd3dDevice->DrawIndexedPrimitive(D3DPT_TRIANGLELIST,0,0,NumVertices,0,PrimCount));  

	//	m_pd3dDevice->SetRenderState(D3DRS_FILLMODE,D3DFILL_WIREFRAME);

	//	mesh->DrawSubset(0);
	//	
	//}
	
	
	//IFC(m_pd3dDevice->SetTexture(0,nullptr));
	//D3DXMATRIXA16 tmp;
	//D3DXMatrixIdentity(&tmp);
	//IFC(m_pd3dDevice->SetTransform(D3DTS_WORLD, &tmp));

	//m_pd3dDevice->SetSamplerState(0, D3DSAMP_MIPFILTER, D3DTEXF_NONE);


	m_pd3dDevice->SetRenderState( D3DRS_CULLMODE, D3DCULL_CW );


	m_pd3dDevice->SetRenderState(D3DRS_FILLMODE,D3DFILL_SOLID);
Cleanup:

    return hr;

}


void
CPCustom::calWorld()
{



	//位移到指定经纬度高
	float distanceGround;
	distanceGround=0;

	D3DXMATRIXA16 tmp;


	D3DXMatrixScaling(&world,scaleX,scaleY,scaleZ);

	D3DXMatrixRotationAxis(&tmp, &axis, angle); 
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


//------------------------------------------------
//拾取模型，返回id
//------------------------------------------------
int
CPCustom::Pick_Model(IDirect3DDevice9* pd3dDevice, POINT ptCursor)
{
	int result=0;
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


void
CPCustom::ChangeProperty(EModelType modeltype, EPropertyType propertytype,void* para,int count)
{
	if (modeltype==自定义模型)
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
		}
		else if (propertytype==模式)
		{
			memcpy(&drawMode,(EDrawMode*)para,sizeof(drawMode));
		}
		else if (propertytype==长度)
		{
			memcpy(&saveX,(float*)para,sizeof(saveX));
			scaleX=saveX;//*progress;
			calWorld();
		}
		else if (propertytype==宽度)
		{
			memcpy(&saveY,(float*)para,sizeof(saveY));
			scaleY=saveY;//*progress;
			calWorld();
		}
		else if (propertytype==高度)
		{
			memcpy(&saveZ,(float*)para,sizeof(saveZ));
			scaleZ=saveZ;//*progress;
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
		else if (propertytype==角度)
		{
			memcpy(&angle,(float*)para,sizeof(angle));
			calWorld();
		}
	}
	


}

