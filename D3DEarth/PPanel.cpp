#include "StdAfx.h"
#include "PPanel.h"


CPPanel::CPPanel(void)
{
}


CPPanel::~CPPanel(void)
{
}


HRESULT
CPPanel::Create(IDirect3DDevice9 *m_pd3dDevice, CBasicModel **ppModel, int id , void* ppara, void* pmesh, int mcount, void* ptexture, int tcount, CTriangleRenderer* tri)
{
	HRESULT hr = S_OK;

	CPPanel *pModel = new CPPanel();
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
CPPanel::Init(IDirect3DDevice9 *m_pd3dDevice, void* ppara, void* pmesh, int mcount, void* ptexture, int tcount)
{
	HRESULT hr = S_OK;

	STRUCT_Area* para=(STRUCT_Area*)ppara;
	this->isReceivePick=para->isReceivePick;
	this->pickFlagId=para->pickFlagId;
	this->deepOrd=para->deepOrd;
	this->aniShow=para->aniShow;

	this->axis=para->axis;
	this->angle=para->angle;

	g_pTexture=NULL;
	D3DXVECTOR3* pd=(D3DXVECTOR3*)pmesh;

	CUSTOMVERTEX_T* vertices(nullptr);
	vertices=new CUSTOMVERTEX_T[mcount];
	for (int i=0; i<mcount; i++)
	{
		vertices[i].position= D3DXVECTOR3(pd[i].x,pd[i].y,pd[i].z);
		D3DXVec3Normalize(&vertices[i].normal, &vertices[i].position);
	}
	vertices[0].u=1 ;vertices[0].v=0;
	vertices[1].u=0 ;vertices[1].v=0;
	vertices[2].u=1 ;vertices[2].v=1;
	vertices[3].u=0 ;vertices[3].v=1;


	//生成材质
    D3DXCreateTextureFromFileInMemory(m_pd3dDevice,ptexture,tcount*sizeof(byte),&g_pTexture);
	//IFC(D3DXCreateTextureFromFile(m_pd3dDevice,L"2.jpg",&g_pTexture));

	NumVertices=mcount; //顶点数
	PrimCount=2; //三角数


	

	//===== 顶点缓冲	
	IFC(m_pd3dDevice->CreateVertexBuffer(NumVertices*sizeof(CUSTOMVERTEX_T), 0, D3DFVF_CUSTOMVERTEX_T, D3DPOOL_DEFAULT, &m_pd3dVB, NULL)); 
	void *pVertices;
	IFC(m_pd3dVB->Lock(0, NumVertices*sizeof(CUSTOMVERTEX_T), &pVertices, 0));
	memcpy(pVertices, vertices, NumVertices*sizeof(CUSTOMVERTEX_T));
	m_pd3dVB->Unlock();


Cleanup:
	delete vertices;
    return hr;

}

HRESULT
CPPanel::Render(IDirect3DDevice9 *m_pd3dDevice)
{
	HRESULT hr = S_OK;



	//m_pd3dDevice->SetSamplerState(0, D3DSAMP_MIPFILTER, D3DTEXF_LINEAR);

	float alpha=1;
	if (isAni)
	{
		UINT  iTime  = GetTickCount()- starttick;
		alpha = (float)iTime / 1000;
		if (alpha>1)
		{
			isAni=false;
		}
	}

	IFC(m_pd3dDevice->SetTransform(D3DTS_WORLD, &world));


	////设置材质
    D3DMATERIAL9 mtrl;
    ZeroMemory( &mtrl, sizeof(D3DMATERIAL9) );
    mtrl.Diffuse.r = mtrl.Ambient.r = 0.0f;
    mtrl.Diffuse.g = mtrl.Ambient.g = 0.0f;
    mtrl.Diffuse.b = mtrl.Ambient.b = 0.0f;
    mtrl.Diffuse.a = mtrl.Ambient.a = alpha;//0.5f;
    m_pd3dDevice->SetMaterial( &mtrl );
	//m_pd3dDevice->SetRenderState( D3DRS_LIGHTING, TRUE );
 //   m_pd3dDevice->SetRenderState( D3DRS_AMBIENT, 0xffffffff);

	
	//一次渲染：新等值图渐现
	m_pd3dDevice->SetTextureStageState(0,D3DTSS_COLORARG1,D3DTA_TEXTURE);
	
	m_pd3dDevice->SetTextureStageState(0,D3DTSS_ALPHAARG1,D3DTA_DIFFUSE);
	m_pd3dDevice->SetTextureStageState(0,D3DTSS_ALPHAARG2,D3DTA_TEXTURE);
	m_pd3dDevice->SetTextureStageState(0,D3DTSS_ALPHAOP,D3DTOP_MODULATE);

	m_pd3dDevice->SetRenderState(D3DRS_ALPHABLENDENABLE,true);
	m_pd3dDevice->SetRenderState(D3DRS_SRCBLEND,D3DBLEND_SRCALPHA);
	m_pd3dDevice->SetRenderState(D3DRS_DESTBLEND,D3DBLEND_INVSRCALPHA);

	IFC(m_pd3dDevice->SetTexture(0,g_pTexture));
	IFC(m_pd3dDevice->SetStreamSource(0, m_pd3dVB, 0, sizeof(CUSTOMVERTEX_T)));
    IFC(m_pd3dDevice->SetFVF(D3DFVF_CUSTOMVERTEX_T));
	IFC(m_pd3dDevice->DrawPrimitive(D3DPT_TRIANGLESTRIP, 0, PrimCount));   

	if (g_pTexture2!=NULL && isAni)  //二次渲染，旧等值图渐隐
	{
		mtrl.Diffuse.a = mtrl.Ambient.a = 1-alpha;//0.5f;
	    m_pd3dDevice->SetMaterial( &mtrl );


		IFC(m_pd3dDevice->SetTexture(0,g_pTexture2));
		m_pd3dDevice->SetTextureStageState(0,D3DTSS_ALPHAARG1,D3DTA_DIFFUSE);
		m_pd3dDevice->SetTextureStageState(0,D3DTSS_ALPHAARG2,D3DTA_TEXTURE);
		m_pd3dDevice->SetTextureStageState(0,D3DTSS_ALPHAOP,D3DTOP_MODULATE);

		IFC(m_pd3dDevice->SetStreamSource(0, m_pd3dVB, 0, sizeof(CUSTOMVERTEX_T)));
	    IFC(m_pd3dDevice->SetFVF(D3DFVF_CUSTOMVERTEX_T));
		IFC(m_pd3dDevice->DrawPrimitive(D3DPT_TRIANGLESTRIP, 0, PrimCount));   
	}




	IFC(m_pd3dDevice->SetTexture(0,nullptr));




	//m_pd3dDevice->SetSamplerState(0, D3DSAMP_MIPFILTER, D3DTEXF_NONE);
Cleanup:

    return hr;

}

void
CPPanel::SetEffect(IDirect3DDevice9 *m_pd3dDevice,CTriangleRenderer* myTri)
{


}

void
CPPanel::ChangeProperty(EPropertyType propertytype,void* para,int count)
{
	if (propertytype==纹理)
	{
		if (g_pTexture!=NULL)
		{
			if (g_pTexture2!=NULL)
				g_pTexture2->Release();
			g_pTexture2=g_pTexture;
		}
		D3DXCreateTextureFromFileInMemory(myTri->m_pd3dDevice,para,count*sizeof(byte),&g_pTexture);
		//D3DXCreateTextureFromFileInMemoryEx(myTri->m_pd3dDevice,para,count*sizeof(byte),0,0,0,0,D3DFMT_DXT1,D3DPOOL_DEFAULT,D3DX_FILTER_BOX,D3DX_DEFAULT,0xff000000,0,0 ,&g_pTexture);
		starttick= GetTickCount();
		isAni=true;

	}
}