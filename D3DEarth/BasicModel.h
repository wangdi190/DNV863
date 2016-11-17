#pragma once
class CBasicModel
{
public:
	CBasicModel(void);
	~CBasicModel(void);
	virtual HRESULT Render(IDirect3DDevice9 *m_pd3dDevice)=0;
	virtual void calWorld();

	D3DXMATRIXA16 world;
	map<int,CBasicModel*> submodels;

	CTriangleRenderer* myTri;

	int operate; //操作 0:初始状态 1:增加 2:已存在
	int status; //状态 0:初始状态 1: 已载入
	int id;
	int rootid;
	int parentid;

	bool isReceivePick;  //是否接受拾取
	int pickFlagId;  //拾取的标志ID，应用于限制指定标志的拾取

	static CBasicModel* find(int rootid, int findid, CTriangleRenderer* myTri);
	CBasicModel* find(int findid);

	UINT recentUseTime;

	int Pick_ModelMesh(IDirect3DDevice9* pd3dDevice, POINT ptCursor, LPD3DXMESH testmesh);
	virtual int Pick_Model(IDirect3DDevice9* pd3dDevice, POINT ptCursor);

	void setOperate(int op); //设置操作状态，包括子对象
	void clearSub(int op);// 清理子对象中operate=op的项

	//void changeColor(DWORD color);

	int deepOrd;

	D3DXVECTOR3 axis; //初始旋转轴
	float angle; //初始旋转角度


protected:
	
	LPD3DXMESH mesh;

	bool isInner; //是否是内部模型，受父对象属性影响，不受外界直接控制，典型的是潮流箭头

	UINT NumVertices; //顶点数
	UINT PrimCount; //三角数
	IDirect3DVertexBuffer9 *m_pd3dVB;  //顶点缓存
	IDirect3DIndexBuffer9 *m_pd3dIB;  //索引缓存

	bool isDicTexture;
	bool isDicTexture2;
	LPDIRECT3DTEXTURE9	g_pTexture; //纹理
	LPDIRECT3DTEXTURE9	g_pTexture2; //纹理

	D3DMATERIAL9 mtrl;  //材质，用于色彩
	

private:

};

