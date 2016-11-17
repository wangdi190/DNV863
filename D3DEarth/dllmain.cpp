// dllmain.cpp : Defines the entry point for the DLL application.
#include "stdafx.h"
using namespace std;


BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

CTriangleRenderer* CGlobal::pCurRender;

static CRendererManager *pManager = NULL;

static HRESULT EnsureRendererManager()
{
    return pManager ? S_OK : CRendererManager::Create(&pManager);
}

extern "C" HRESULT WINAPI SetSize(int ekey, UINT uWidth, UINT uHeight)
{
    HRESULT hr = S_OK;

    IFC(EnsureRendererManager());

	pManager->SetCurrentTriangle(ekey);

    pManager->SetSize(uWidth, uHeight);

Cleanup:
    return hr;
}

extern "C" HRESULT WINAPI SetAlpha(int ekey,BOOL fUseAlpha)
{
    HRESULT hr = S_OK;

    IFC(EnsureRendererManager());
	pManager->SetCurrentTriangle(ekey);

    pManager->SetAlpha(!!fUseAlpha);

Cleanup:
    return hr;
}

extern "C" HRESULT WINAPI SetNumDesiredSamples(int ekey,UINT uNumSamples)
{
    HRESULT hr = S_OK;

    IFC(EnsureRendererManager());
	pManager->SetCurrentTriangle(ekey);

    pManager->SetNumDesiredSamples(uNumSamples);

Cleanup:
    return hr;
}

extern "C" HRESULT WINAPI SetAdapter(int ekey,POINT screenSpacePoint)  //多卡未处理
{
    HRESULT hr = S_OK;

    IFC(EnsureRendererManager());
	
    pManager->SetAdapter(ekey, screenSpacePoint);

Cleanup:
    return hr;
}

extern "C" HRESULT WINAPI GetBackBufferNoRef(int ekey,IDirect3DSurface9 **ppSurface)
{
    HRESULT hr = S_OK;

    IFC(EnsureRendererManager());
	pManager->SetCurrentTriangle(ekey);
    IFC(pManager->GetBackBufferNoRef(ekey,ppSurface));

Cleanup:
    return hr;
}

extern "C" HRESULT WINAPI Render(int ekey)
{
    assert(pManager);
	pManager->SetCurrentTriangle(ekey);

    return pManager->Render();
}

extern "C" void WINAPI Destroy(int ekey)
{
	pManager->DelTriangle(ekey);

	if (pManager->Triangles.size()==0)
	{
		delete pManager;
		pManager = NULL;
	}
}

extern "C" HRESULT WINAPI AddMapTile(int ekey,int id, int layer, int idxx, int idxy, bool isShowTerrain, int terrainSpan, void* pHigh)
{
    HRESULT hr = S_OK;

    IFC(EnsureRendererManager());
	pManager->SetCurrentTriangle(ekey);

    CGlobal::pCurRender->AddMapTile(id, layer, idxx, idxy, isShowTerrain, terrainSpan, pHigh);

Cleanup:
    return hr;
}


extern "C" HRESULT WINAPI ChangeCameraPara(int ekey,void* pPara, bool isAni, int duration)
{
    HRESULT hr = S_OK;
    IFC(EnsureRendererManager());
	pManager->SetCurrentTriangle(ekey);

    CGlobal::pCurRender->ChangeCameraPara(pPara,isAni,duration);
Cleanup:
    return hr;
}
extern "C" HRESULT WINAPI ChangeLightPara(int ekey, int lightNum,void* pPara )
{
    HRESULT hr = S_OK;
    IFC(EnsureRendererManager());
	pManager->SetCurrentTriangle(ekey);

    CGlobal::pCurRender->ChangeLightPara(lightNum, pPara);
Cleanup:
    return hr;
}
extern "C" HRESULT WINAPI ChangeAmbientLight(int ekey, UINT color )
{
    HRESULT hr = S_OK;
    IFC(EnsureRendererManager());
	pManager->SetCurrentTriangle(ekey);

    CGlobal::pCurRender->ChangeAmbientLight(color);
Cleanup:
    return hr;
}




extern "C" int WINAPI BeginTransfer(int ekey)
{
    HRESULT hr = S_OK;
    IFC(EnsureRendererManager());
	pManager->SetCurrentTriangle(ekey);

    return CGlobal::pCurRender->BeginTransfer();
Cleanup:
    return 0;
}

extern "C" void WINAPI EndTransfer(int ekey)
{
	pManager->SetCurrentTriangle(ekey);

    CGlobal::pCurRender->EndTransfer();
}



extern "C" int WINAPI BeginTransferModel(int ekey)
{
    HRESULT hr = S_OK;
    IFC(EnsureRendererManager());
	pManager->SetCurrentTriangle(ekey);

    return CGlobal::pCurRender->BeginTransferModel();
Cleanup:
    return 0;
}

extern "C" void WINAPI EndTransferModel(int ekey)
{
	pManager->SetCurrentTriangle(ekey);
    CGlobal::pCurRender->EndTransferModel();
}

//modeltype 模型类型, 
//pid 模型id, 
//para 参数结构
//pmesh, 用于传递mesh数据
//mcount mesh数据数组大小
//ptexture 用于传递材质数据
//tcount 材质数据数据组大小
extern "C" void WINAPI AddModel(int ekey,int modeltype,int id, void* para, void* pmesh, int mcount,void* ptexture, int tcount)
{
	pManager->SetCurrentTriangle(ekey);
	CGlobal::pCurRender->AddModel((EModelType)modeltype,id , para, pmesh, mcount, ptexture, tcount);
}

//添加自定义模型 internal static extern void AddCostomModel(int ekey, int id, IntPtr para, IntPtr plocation, IntPtr pvertices, IntPtr pnormal, int vcount, IntPtr pindex, int icount); 
extern "C" void WINAPI AddCustomModel(int ekey,int id, void* para, void* plocation, void* pvertices,void* pnormal, int vcount, void* pindex, int icount, void* puv, int uvcount, void* ptexture)
{
	pManager->SetCurrentTriangle(ekey);
	CGlobal::pCurRender->AddCustomModel(id , para, plocation, pvertices, pnormal, vcount, pindex, icount, puv, uvcount, ptexture);
}

extern "C" void WINAPI AddTexture(int ekey,int texturekey, byte* pdatas, int count)
{
	pManager->SetCurrentTriangle(ekey);
	CGlobal::pCurRender->para.AddTexture(texturekey, pdatas, count);
}

extern "C" void WINAPI AddTextureFromFile(int ekey,int texturekey, WCHAR* filepath)
{
	pManager->SetCurrentTriangle(ekey);
	CGlobal::pCurRender->para.AddTexture(texturekey, filepath);
}

//-----------------------------------------------------
// 添加XModel
//-----------------------------------------------------
extern "C" void WINAPI AddXModel(int ekey,int modelkey, WCHAR* filepath, void* axis, float angle)
{
	pManager->SetCurrentTriangle(ekey);
	CGlobal::pCurRender->para.AddXModel(modelkey, filepath, axis, angle);
}

//-----------------------------------------------------
// 添加custom作为XModel
//-----------------------------------------------------
extern "C" void WINAPI AddCustomAsXModel(int ekey,int modelkey, void* pvertices,void* pnormal, int vcount, void* pindex, int icount, void* puv, int uvcount, void* ptexture , void* axis, float angle)
{
	pManager->SetCurrentTriangle(ekey);
	CGlobal::pCurRender->para.AddCustomAsXModel(modelkey, pvertices, pnormal, vcount, pindex,  icount,  puv,  uvcount, ptexture, axis, angle);
}


//-----------------------------------------------------
// 添加几何体
//-----------------------------------------------------
extern "C" void WINAPI AddGeometry(int ekey,int geokey, void* pPara)
{
	pManager->SetCurrentTriangle(ekey);
	CGlobal::pCurRender->para.AddGeometry((EGeometryType)geokey, pPara);
}

//-----------------------------------------------------
// 拾取测试
//-----------------------------------------------------
extern "C" int WINAPI PickModel(int ekey,POINT ptCursor)
{
	pManager->SetCurrentTriangle(ekey);
	return CGlobal::pCurRender->Pick_Model(ptCursor);
}

//-----------------------------------------------------
// 拾取测试, 限定flag
//-----------------------------------------------------
extern "C" int WINAPI PickFlagModel(int ekey,POINT ptCursor,int flagid)
{
	pManager->SetCurrentTriangle(ekey);
	return CGlobal::pCurRender->Pick_Model(ptCursor,flagid);
}

//-----------------------------------------------------
// 更改属性
//-----------------------------------------------------
extern "C" void WINAPI ChangeProperty(int ekey,int modeltype, int propertytype,int id, int subid, void* para, int count, void* para2, int count2)
{
	pManager->SetCurrentTriangle(ekey);

	if ((EModelType)modeltype==图元 && (EPropertyType) propertytype==材质)
	{
		int tmp=0;
	}


	if (CGlobal::pCurRender!=NULL)
	    CGlobal::pCurRender->ChangeProperty((EModelType)modeltype,(EPropertyType) propertytype,id ,subid, para, count, para2, count2);
	
}


//-----------------------------------------------------
// 动态增加单个对象
//-----------------------------------------------------
extern "C" void WINAPI DynAddModel(int ekey,int modeltype,int id, void* para, void* pmesh, int mcount,void* ptexture, int tcount)
{
	pManager->SetCurrentTriangle(ekey);
	CGlobal::pCurRender->AddModel((EModelType)modeltype,id , para, pmesh, mcount, ptexture, tcount);
}

//-----------------------------------------------------
// 返回d3d中点在屏幕位置
//-----------------------------------------------------
extern "C" POINT WINAPI TransformD3DToScreen(int ekey,D3DXVECTOR3 point3d)
{
	pManager->SetCurrentTriangle(ekey);
	return	CHelper::GetProjectPoint2D(point3d, CGlobal::pCurRender->camera->view, CGlobal::pCurRender->camera->projection, CGlobal::pCurRender->para.D3DWidth, CGlobal::pCurRender->para.D3DHeight);
}

