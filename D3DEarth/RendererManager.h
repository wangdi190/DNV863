#pragma once
#include <string>
using namespace std;

class CRenderer;
class CZData;

class CRendererManager
{
public:
    static HRESULT Create(CRendererManager **ppManager);
    ~CRendererManager();

    HRESULT EnsureDevices();

    void SetSize(UINT uWidth, UINT uHeight);
    void SetAlpha(bool fUseAlpha);
    void SetNumDesiredSamples(UINT uNumSamples);
    void SetAdapter(int ekey, POINT screenSpacePoint);

	map<int, CTriangleRenderer*> Triangles;

    HRESULT GetBackBufferNoRef(int ekey,IDirect3DSurface9 **ppSurface);

    HRESULT Render();

	HRESULT SetCurrentTriangle(int ekey);
	void DelTriangle(int ekey);
private:
    CRendererManager();

    void CleanupInvalidDevices();
    HRESULT EnsureRenderers(int ekey);
    HRESULT EnsureHWND();
    HRESULT EnsureD3DObjects();
    HRESULT TestSurfaceSettings();
    void DestroyResources();

    IDirect3D9    *m_pD3D;
    IDirect3D9Ex  *m_pD3DEx;

    UINT m_cAdapters;
    CRenderer **m_rgRenderers;
    CRenderer *m_pCurrentRenderer;

    HWND m_hwnd;

    UINT m_uWidth;
    UINT m_uHeight;
    UINT m_uNumSamples;
    bool m_fUseAlpha;
    bool m_fSurfaceSettingsChanged;

	
};

