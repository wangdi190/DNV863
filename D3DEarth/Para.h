#pragma once
#include <map>
#include <string>
#include "XModel.h"
using namespace std;


class CPara
{
public:
	CPara(void);
	~CPara(void);
	int D3DWidth;
	int D3DHeight;

	float ScalePara;
	float Radius;
	float LineHeight;
	float ArrowHeight;
	float AreaHeight;
	float TextHeight;

	map<int,LPDIRECT3DTEXTURE9> textures;
	map<int,LPD3DXMESH> geometries;
	map<int,CXModel*> xmodels;

	LPD3DXFONT g_pFont2D;       //字体对象
	LPD3DXFONT g_pFont3D;       //字体对象



	void AddTexture(int texturekey, void* pdatas, int count);
	void AddTexture(int texturekey, WCHAR* filepath);

	void AddGeometry(EGeometryType geokey, void* pPara);

	void AddXModel(int modelkey, WCHAR* filepath, void* axis, float angle);
	void AddCustomAsXModel(int modelkey, void* pvertices,void* pnormal, int vcount, void* pindex, int icount, void* puv, int uvcount, void* ptexture,void* axis, float angle);

	UINT m_uWidth;
    UINT m_uHeight;
    UINT m_uNumSamples;
    bool m_fUseAlpha;
    bool m_fSurfaceSettingsChanged;

};

