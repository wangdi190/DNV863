#pragma once
#include <string>


class CMapTile :CBasicModel
{

public:
	static wstring mapIP;   //web方式地址
	static wstring mapPath;  //路径方式  
	static wstring mapPath2;  //路径方式第二地址  overlay
    static HRESULT Create(IDirect3DDevice9 *m_pd3dDevice, CBasicModel **ppModel ,int id, int zlayer, int idxx,int idxy, bool isShowTerrain, int terrainSpan, void* pHigh, CTriangleRenderer* tri);
	~CMapTile(void);
    HRESULT Render(IDirect3DDevice9 *m_pd3dDevice);
	wstring QuadKey;
	int id;
	int layer,idxX,idxY;

	HRESULT Init();

	static void SetEffect(IDirect3DDevice9 *m_pd3dDevice,CTriangleRenderer* myTri);

	void SetTerrain(bool IsShowTerrain, int TerrainSpan, void* PHigh);
	bool isShowTerrain;
	int terrainSpan;

protected:
    //HRESULT Init(IDirect3DDevice9 *m_pd3dDevice);

private:
	
   	CMapTile(void);

	void CheckTexture(); //检查纹理状态，决定是否重新载入

	HRESULT GetTexture(IDirect3DDevice9 *m_pd3dDevice); //读取材质文件
	HRESULT GetOverlayTexture(IDirect3DDevice9 *m_pd3dDevice); //读取材质文件

	EMapType curMapType; 
	bool curIsShowOverlay;

	HRESULT buildPlaneMesh();
	HRESULT buildTerrainMesh(void* pHigh);
	UINT TerrainNumVertices; //顶点数
	UINT TerrainPrimCount; //三角数
	IDirect3DVertexBuffer9 *Terrain_pd3dVB;  //顶点缓存
	IDirect3DIndexBuffer9 *Terrain_pd3dIB;  //索引缓存
	float* pHigh;
};



