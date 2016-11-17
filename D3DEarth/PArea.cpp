#include "StdAfx.h"
#include "PArea.h"


CPArea::CPArea(void)
{
}


CPArea::~CPArea(void)
{
}

HRESULT
CPArea::Create(IDirect3DDevice9 *m_pd3dDevice, CBasicModel **ppModel, int id , void* ppara, void* pmesh, int mcount, void* ptexture, int tcount, CTriangleRenderer* tri)
{
	HRESULT hr = S_OK;

	CPArea *pModel = new CPArea();
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
CPArea::Init(IDirect3DDevice9 *m_pd3dDevice, void* ppara, void* pmesh, int mcount, void* ptexture, int tcount)
{
	HRESULT hr = S_OK;

	STRUCT_Area* para=(STRUCT_Area*)ppara;
    this->isReceivePick=para->isReceivePick;
	this->pickFlagId=para->pickFlagId;
	this->deepOrd=para->deepOrd;
	//this->color=para->color;
	memcpy(&mtrl,&para->material,sizeof(mtrl));
	this->aniShow=para->aniShow;

	this->axis=para->axis;
	this->angle=para->angle;

	buildMesh( pmesh,  mcount, ptexture, tcount);


	////===== 顶点缓冲	
	//IFC(m_pd3dDevice->CreateVertexBuffer(NumVertices*sizeof(CUSTOMVERTEX_C), 0, D3DFVF_CUSTOMVERTEX_C, D3DPOOL_DEFAULT, &m_pd3dVB, NULL)); 
	//void *pVertices;
	//IFC(m_pd3dVB->Lock(0, NumVertices*sizeof(CUSTOMVERTEX_C), &pVertices, 0));
	//memcpy(pVertices, vertices, NumVertices*sizeof(CUSTOMVERTEX_C));
	//m_pd3dVB->Unlock();
	////===== 索引缓冲
	//IFC(m_pd3dDevice->CreateIndexBuffer(PrimCount*3*sizeof(WORD), 0, D3DFMT_INDEX16 , D3DPOOL_DEFAULT, &m_pd3dIB, NULL)); 
	//void *pIndexes;
	//IFC(m_pd3dIB->Lock(0, PrimCount*3*sizeof(WORD), &pIndexes, 0));
	//memcpy(pIndexes, idxes, PrimCount*3*sizeof(WORD));
	//m_pd3dIB->Unlock();


	////设置材质
//   ZeroMemory( &mtrl, sizeof(D3DMATERIAL9) );
	//D3DXCOLOR c=D3DXCOLOR(color);

	//mtrl.Diffuse.r = mtrl.Ambient.r = c.r;//1.0f;
 //   mtrl.Diffuse.g = mtrl.Ambient.g =c.g;// 0.0f;
 //   mtrl.Diffuse.b = mtrl.Ambient.b =c.b;// 0.0f;
 //   mtrl.Diffuse.a = mtrl.Ambient.a =c.a;// 1.0f;


Cleanup:
	//delete vertices;
	//delete idxes;

    return hr;

}

//--------------------------------------------------------------------
// 根据点集，创建mesh
//--------------------------------------------------------------------
void 
CPArea::buildMesh(void* pmesh, int mcount, void* ptexture, int tcount)
{
	D3DXVECTOR3* pd=(D3DXVECTOR3*)pmesh;

	CUSTOMVERTEX_N* vertices(nullptr);
	vertices=new CUSTOMVERTEX_N[mcount];

	for (int i=0; i<mcount; i++)
	{
		vertices[i].position=D3DXVECTOR3(pd[i].x,pd[i].y,pd[i].z);
		vertices[i].normal=D3DXVECTOR3(pd[i].x,pd[i].y,pd[i].z);
		D3DXVec3Normalize(&vertices[i].normal,&vertices[i].normal);
	}

	WORD* pi=(WORD*)ptexture;
	WORD* idxes(nullptr);
	idxes=new WORD[tcount];
	memcpy(idxes,pi,sizeof(pi[0])*tcount); //复制索引, 注：借用了材质指针

	NumVertices=mcount; //顶点数
	PrimCount=tcount/3; //三角数


		// 建立mesh
	if (mesh!=NULL)
		mesh->Release();

	D3DXCreateMeshFVF(PrimCount,NumVertices,D3DXMESH_DYNAMIC, D3DFVF_CUSTOMVERTEX_N,myTri->m_pd3dDevice, &mesh);
	
	CUSTOMVERTEX_N* v = 0;
	mesh->LockVertexBuffer(0, (void**)&v);
	for (int i=0;i<NumVertices;i++)
		v[i]=vertices[i];
	mesh->UnlockVertexBuffer();

 
	WORD* idx = 0;
	mesh->LockIndexBuffer(0, (void**)&idx);
	for (int i=0;i<tcount;i++)
	{
		idx[i]=idxes[i];
	}
	mesh->UnlockIndexBuffer();
	//优化
	vector<DWORD> adjacencyBuffer(mesh->GetNumFaces() * 3);
	mesh->GenerateAdjacency(0.0f, &adjacencyBuffer[0]);
	mesh->OptimizeInplace(D3DXMESHOPT_ATTRSORT|D3DXMESHOPT_COMPACT|D3DXMESHOPT_VERTEXCACHE, &adjacencyBuffer[0],0, 0, 0);

	Cleanup:
	delete vertices;
	delete idxes;
}

HRESULT
CPArea::Render(IDirect3DDevice9 *m_pd3dDevice)
{
	HRESULT hr = S_OK;

	//IFC(m_pd3dDevice->SetStreamSource(0, m_pd3dVB, 0, sizeof(CUSTOMVERTEX_C)));
 //   IFC(m_pd3dDevice->SetFVF(D3DFVF_CUSTOMVERTEX_C));
	//IFC(m_pd3dDevice->SetIndices(m_pd3dIB));  
	//IFC(m_pd3dDevice->DrawIndexedPrimitive(D3DPT_TRIANGLELIST,0,0,NumVertices,0,PrimCount));   



    //m_pd3dDevice->SetRenderState( D3DRS_LIGHTING, TRUE );
    //m_pd3dDevice->SetRenderState( D3DRS_AMBIENT, 0xffffffff);

	CGlobal::pCurRender->m_pd3dDevice->SetRenderState( D3DRS_CULLMODE, D3DCULL_CCW );

	m_pd3dDevice->SetMaterial( &mtrl );

	if (mesh!=NULL)
		mesh->DrawSubset(0);


	for (auto iter=submodels.begin();iter!=submodels.end();iter++)
		((CBasicModel*)(iter->second))->Render(m_pd3dDevice);

	CGlobal::pCurRender->m_pd3dDevice->SetRenderState( D3DRS_CULLMODE, D3DCULL_CW );

Cleanup:

    return hr;

}

void
CPArea::SetEffect(IDirect3DDevice9 *m_pd3dDevice,CTriangleRenderer* myTri)
{


}

void
CPArea::ChangeProperty(EModelType modeltype, EPropertyType propertytype,void* para,int count, void* para2, int count2)
{
		if (propertytype==材质)
		{
			memcpy(&mtrl,(D3DMATERIAL9*)para,sizeof(mtrl));
			//changeColor(color);
		}
		else if (propertytype==位置)
		{
			buildMesh( para,  count, para2, count2);
		}
}
