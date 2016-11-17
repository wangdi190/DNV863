#pragma once
#include "basicmodel.h"
#include "PPolyCol.h"
class CPSymbol :public CBasicModel
{
public:
	~CPSymbol(void);
	static HRESULT Create(IDirect3DDevice9 *m_pd3dDevice, CBasicModel **ppModel, int id , void* ppara, void* pmesh, int mcount, void* ptexture, int tcount,CTriangleRenderer* tri);
	HRESULT Init(IDirect3DDevice9 *m_pd3dDevice, void* ppara, void* pmesh, int mcount, void* ptexture, int tcount);
	void ChangeProperty(EModelType modeltype, EPropertyType propertytype,void* para,int count);
    HRESULT Render(IDirect3DDevice9 *m_pd3dDevice);
	static void SetEffect(IDirect3DDevice9 *m_pd3dDevice,CTriangleRenderer* myTri);

	D3DXVECTOR3 location;
	
	CPPolyCol* pparent;
	void calWorld();

	int Pick_Model(IDirect3DDevice9* pd3dDevice, POINT ptCursor);

private:
    CPSymbol(void);
	
	int texturekey;
	float scaleX;
	float scaleY;
	float scaleZ;
	float saveX;
	float saveY;
	float saveZ;
	bool isH;
	//DWORD color;
	bool isUseColor;
	bool isUseXModel;  //是否使用.X 3D模型
	int XMKey; //.X 3D模型键，与材质类似
	float XMScaleAddition; //3D模型时的附加比例系数

	STRUCT_Ani aniTwinkle; //闪烁动画参数;
	STRUCT_Ani aniShow; //渐变显示动画参数;
	STRUCT_Ani aniScale; //缩放动画参数;

	float progress;//图形显示的百分比进度

	float savealpha;
};

