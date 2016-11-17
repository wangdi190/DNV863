#pragma once
#include "basicmodel.h"
class CPCustom:public CBasicModel
{
public:
	~CPCustom(void);
	static HRESULT Create(IDirect3DDevice9 *m_pd3dDevice, CBasicModel **ppModel, int id , void* ppara,void* plocation, void* pvertices, void* pnormal, int vcount, void* pindex, int icount, void* puv, int uvcount, void* ptexture, CTriangleRenderer* tri);
	HRESULT Init(IDirect3DDevice9 *m_pd3dDevice, void* ppara,void* plocation, void* pvertices, void* pnormal, int vcount, void* pindex, int icount, void* puv, int uvcount, void* ptexture);
    HRESULT Render(IDirect3DDevice9 *m_pd3dDevice);
    void ChangeProperty(EModelType modeltype, EPropertyType propertytype,void* para,int count);


	D3DXVECTOR3 location;
	
	void calWorld();

	int Pick_Model(IDirect3DDevice9* pd3dDevice, POINT ptCursor);

private:
    CPCustom(void);
	
	void buildMesh(void* pvertices, void* pnormal, int vcount, void* pindex, int icount, void* puv, int uvcount);
	int texturekey;
	float scaleX;
	float scaleY;
	float scaleZ;
	float saveX;
	float saveY;
	float saveZ;
	//DWORD color;

	wstring texture;
	EDrawMode drawMode;

};

