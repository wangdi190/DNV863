#pragma once
class CXModel
{
public:
	CXModel(void);
	~CXModel(void);


	LPD3DXMESH              g_pMesh;  //网格模型对象
	D3DMATERIAL9*           g_pMeshMaterials;  //网格模型材质
	LPDIRECT3DTEXTURE9*     g_pMeshTextures;  //网格模型纹理
	DWORD                   g_dwNumMaterials;    //网格模型材质数量

	int modelkey;
	wstring xfile;

	D3DXVECTOR3 axis; //初始旋转轴
	float angle; //初始旋转角度

	HRESULT InitGeometry();  //初始x文件提供的模型
	void InitCustom(void* pvertices,void* pnormal, int vcount, void* pindex, int icount, void* puv, int uvcount, void* ptexture); //初始化custom类型的做为模型
	void RemovePathFromFileName(LPSTR fullPath, LPWSTR fileName);

	void Cleanup();

	void Render();

	bool isCustom; //是X形式，或是custom形式

	//下为custom所用
	//LPD3DXMESH mesh;
	LPDIRECT3DTEXTURE9	g_pTexture; //纹理
	D3DMATERIAL9 mtrl;  //材质，用于色彩


};

