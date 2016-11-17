#include "StdAfx.h"
#include "Para.h"


CPara::CPara(void)
{
ScalePara=1.0f;
Radius=6378.137f*ScalePara;
LineHeight=0.0005f*ScalePara;
ArrowHeight=0.00051f*ScalePara;
AreaHeight=0.0004f*ScalePara;
TextHeight=0.00053f*ScalePara;
g_pFont2D=0;
g_pFont3D=0;



}


CPara::~CPara(void)
{
	CPara::g_pFont2D->Release();
	CPara::g_pFont3D->Release();

	for	(auto iter=textures.begin();iter!=textures.end();iter++)
	{
		iter->second->Release();
	}
	for	(auto iter=geometries.begin();iter!=geometries.end();iter++)
	{
		iter->second->Release();
	}
	for (auto iter=xmodels.begin();iter!=xmodels.end();iter++)
	{
		iter->second->Cleanup();
	}

}


void
CPara::AddTexture(int texturekey, void* pdatas, int count)
{
	LPDIRECT3DTEXTURE9 g_pTexture; //材质
    D3DXCreateTextureFromFileInMemory(CGlobal::pCurRender->m_pd3dDevice,pdatas,count*sizeof(byte),&g_pTexture);
	//D3DXCreateTextureFromFileInMemoryEx(CGlobal::pCurRender->m_pd3dDevice,pdatas,count*sizeof(byte),0,0,0,0,D3DFMT_DXT1,D3DPOOL_DEFAULT,D3DX_FILTER_BOX,D3DX_DEFAULT,0xff000000,0,0 ,&g_pTexture);

	CPara::textures.insert(EntryTexture(texturekey,g_pTexture));
}

void
CPara::AddTexture(int texturekey, WCHAR* filepath)
{
	if (textures.find(texturekey)==textures.end())
	{
	LPDIRECT3DTEXTURE9 g_pTexture; //材质
	wstring file(filepath);
	D3DXCreateTextureFromFile(CGlobal::pCurRender->m_pd3dDevice,file.c_str(),&g_pTexture);
	//D3DXCreateTextureFromFileEx(
 //    CGlobal::pCurRender->m_pd3dDevice,
 //    file.c_str(),
 //    D3DX_DEFAULT,
 //    D3DX_DEFAULT,
 //    D3DX_DEFAULT,
 //    0,
 //    D3DFMT_A1R5G5B5,
 //    D3DPOOL_DEFAULT,
 //    D3DX_FILTER_TRIANGLE,
 //    D3DX_FILTER_TRIANGLE,
 //    D3DCOLOR_RGBA(0,0,0,255),
 //    NULL,
 //    NULL,
 //    &g_pTexture);
	CPara::textures.insert(EntryTexture(texturekey,g_pTexture));
	}
}

void
CPara::AddGeometry(EGeometryType geokey, void* pPara)
{
	STRUCT_Geometry* para=(STRUCT_Geometry*)pPara;
	ID3DXMesh* mesh=0;
	if (para->geoType==立方体)
	{
		D3DXCreateBox(CGlobal::pCurRender->m_pd3dDevice,para->pf1,para->pf2,para->pf3,&mesh,NULL);
	}
	else if (para->geoType==柱体)
	{
		D3DXCreateCylinder(CGlobal::pCurRender->m_pd3dDevice,para->pf1,para->pf2,para->pf3,para->pi1,para->pi2,&mesh,NULL);
	}
	else if (para->geoType==球体)
	{
		D3DXCreateSphere(CGlobal::pCurRender->m_pd3dDevice,para->pf1,para->pi1,para->pi2,&mesh,NULL);
	}
	CPara::geometries.insert(EntryGeometry(geokey,mesh));
}


void
CPara::AddXModel(int modelkey, WCHAR* filepath, void* axis, float angle)
{
	CXModel* xmodel=new CXModel(); 
	wstring file(filepath);
	xmodel->xfile=file;
	
	D3DXVECTOR3* paxis=(D3DXVECTOR3*)axis;
	memcpy(&xmodel->axis,paxis,sizeof(xmodel->axis)); //复制
	xmodel->angle=angle;

	xmodel->InitGeometry();
	CPara::xmodels.insert(EntryXModel(modelkey,xmodel));
}

void 
CPara::AddCustomAsXModel(int modelkey, void* pvertices,void* pnormal, int vcount, void* pindex, int icount, void* puv, int uvcount, void* ptexture, void* axis, float angle)
{
	CXModel* xmodel=new CXModel(); 
	
	D3DXVECTOR3* paxis=(D3DXVECTOR3*)axis;
	memcpy(&xmodel->axis,paxis,sizeof(xmodel->axis)); //复制
	xmodel->angle=angle;

	xmodel->InitCustom(pvertices,pnormal,vcount,pindex,icount,puv,uvcount,ptexture);
	CPara::xmodels.insert(EntryXModel(modelkey,xmodel));

}