#pragma once
#include "basicmodel.h"
class CPPanel :	CBasicModel
{
public:
	~CPPanel(void);

	static HRESULT Create(IDirect3DDevice9 *m_pd3dDevice, CBasicModel **ppModel, int id , void* ppara, void* pmesh, int mcount, void* ptexture, int tcount,CTriangleRenderer* tri);
	HRESULT Init(IDirect3DDevice9 *m_pd3dDevice, void* ppara, void* pmesh, int mcount, void* ptexture, int tcount);
    HRESULT Render(IDirect3DDevice9 *m_pd3dDevice);
	static void SetEffect(IDirect3DDevice9 *m_pd3dDevice,CTriangleRenderer* myTri);

	void ChangeProperty(EPropertyType propertytype,void* para,int count);

private:
	CPPanel(void);

	UINT starttick; //开始时的计时数
	bool isAni;

	STRUCT_Ani aniShow; //闪烁动画参数;


};

