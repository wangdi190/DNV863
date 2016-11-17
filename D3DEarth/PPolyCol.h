#pragma once
#include "basicmodel.h"
class CPPolyCol :public CBasicModel
{
public:
	static HRESULT Create(IDirect3DDevice9 *m_pd3dDevice, CBasicModel **ppModel,  int id , void* ppara, void* pmesh, int mcount, void* ptexture, int tcount,CTriangleRenderer* tri);
	~CPPolyCol(void);
    HRESULT Render(IDirect3DDevice9 *m_pd3dDevice);
	void calWorld();

	HRESULT Init(IDirect3DDevice9 *m_pd3dDevice,void* ppara, void* pmesh, int mcount, void* ptexture, int tcount);
	void ChangeProperty(EPropertyType propertytype,void* para,int count);

	CPPolyCol* pparent;


	D3DXVECTOR3 location;
	float distanceGround;
	float height;

	int geokey;
	static void SetEffect(IDirect3DDevice9 *m_pd3dDevice,CTriangleRenderer* myTri);
		
private:
	CPPolyCol(void);
	//DWORD color;
	float sizex;
	float sizey;

	float targetHeight; //目标值
	float divHeight; //高差
	UINT starttick; //开始时的计时数
	bool isAni;
	
	STRUCT_Ani aniScale; //闪烁动画参数;
	STRUCT_Ani aniRotation; //旋转动画参数;

	float rotateAngle; //旋转角度
};

