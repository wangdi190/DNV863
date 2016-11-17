#pragma once
#include "renderer.h"



class CPline : CBasicModel
{
public:
	static HRESULT Create(IDirect3DDevice9 *m_pd3dDevice, CBasicModel **ppModel, int id ,void* ppara, void* pdots, int dotcount,CTriangleRenderer* tri);
	~CPline(void);
    HRESULT Render(IDirect3DDevice9 *m_pd3dDevice);

	HRESULT Init(IDirect3DDevice9 *m_pd3dDevice,void* ppara, void* pdots, int dotcount);
	void ChangeProperty(EModelType modeltype, EPropertyType propertytype,void* para,int count);

	D3DXVECTOR3* points;

	int pointCount;

	void showFlow(bool isshow);
	DWORD arrowColor;
	float arrowSize; 
	bool isInverse;

	int radCount; //线柱的分片数

	STRUCT_Ani aniFlow; //潮流动画参数
	float lenth;  //线路长度
    D3DXVECTOR3* dirs; //线段归一化方向
    float* lens; //线段长度


	static void SetEffect(IDirect3DDevice9 *m_pd3dDevice,CTriangleRenderer* myTri);

private:
	CPline(void);

	STRUCT_Ani aniDraw; //绘制动画参数
	STRUCT_Ani aniTwinkle; //闪烁动画参数;
	//DWORD color;
	float thickness; //线宽
	float savealpha; //原始色的Alpah值

	void buildMesh();
	void getPosition(float progress, int* passcount, D3DXVECTOR3* pos);  //根据progress，计算动画走到的位置
	void buildVerticBuffer(int passcount,  D3DXVECTOR3 pos);  //创建绘制动画的顶点buffer

	CUSTOMVERTEX_N* vertices;  //绘制动画用顶点
	WORD* idxes; //绘制动画用索引

	float progress;//图形显示的百分比进度
};

