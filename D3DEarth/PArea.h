#pragma once
#include "basicmodel.h"
class CPArea :	CBasicModel
{
public:
	~CPArea(void);
	static HRESULT Create(IDirect3DDevice9 *m_pd3dDevice, CBasicModel **ppModel, int id , void* ppara, void* pmesh, int mcount, void* ptexture, int tcount,CTriangleRenderer* tri);
	HRESULT Init(IDirect3DDevice9 *m_pd3dDevice, void* ppara, void* pmesh, int mcount, void* ptexture, int tcount);
    HRESULT Render(IDirect3DDevice9 *m_pd3dDevice);
	static void SetEffect(IDirect3DDevice9 *m_pd3dDevice,CTriangleRenderer* myTri);

	void ChangeProperty(EModelType modeltype, EPropertyType propertytype,void* para,int count,void* para2, int count2);

private:
	CPArea(void);

	void buildMesh(void* pmesh, int mcount, void* ptexture, int tcount);

	//DWORD color;
	STRUCT_Ani aniShow; //¶¯»­²ÎÊý;

};

