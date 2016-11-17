#pragma once
#include "basicmodel.h"
#include "PPolyCol.h"
class CPText :public CBasicModel
{
public:
	~CPText(void);
	static HRESULT Create(IDirect3DDevice9 *m_pd3dDevice, CBasicModel **ppModel, int id , void* ppara, void* pmesh, int mcount, void* ptext, int tcount,CTriangleRenderer* tri);
	HRESULT Init(IDirect3DDevice9 *m_pd3dDevice, void* ppara, void* pmesh, int mcount, void* ptext, int tcount);
    HRESULT Render(IDirect3DDevice9 *m_pd3dDevice);
	static void SetEffect(IDirect3DDevice9 *m_pd3dDevice,CTriangleRenderer* myTri);
	void ChangeProperty(EPropertyType propertytype,void* para,int count);
	D3DXVECTOR3 location;
	
	CPPolyCol* pparent;
	void calWorld();

private:
    CPText(void);
	
	vector<wstring> text;
	
	int texturekey;//材质背景

	//DWORD color;
	float scalex; //传入的x方向缩放修正
	float scaley; //传入的y方向缩放修正

	float width; //实际使用宽度
	float height; //实际使用高度
	bool isH;
	bool is3D;

	STRUCT_Ani aniTwinkle; //闪烁动画参数;

	HRESULT CreateTexture(IDirect3DDevice9 *m_pd3dDevice);

	RECT getScreenRect(D3DXVECTOR3 loca);
};

