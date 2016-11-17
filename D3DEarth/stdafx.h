// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once
enum EGeometryType {立方体,柱体,球体};

enum ETileReadMode { 内置瓦片服务, 自定义文件瓦片, 自定义Web瓦片};
enum EAniType {无动画,闪烁,绘制,擦除,潮流动画,渐变,缩放,旋转};
enum EMapType {卫星,道路,地形,无};
enum EModelType{地图,相机,光源,折线,图元,几何体,区域,等值图,文字,潮流,自定义模型};
enum EPropertyType {类型,颜色,大小,长度,宽度,高度,偏移,方向,纹理,材质,动画,内容,可见性,参数,地址,路径,路径2,进度,位置,角度,模式};

enum EInputCoordinate { WGS84球面坐标, 平面坐标 };
enum ESceneMode { 地球, 局部平面 };
enum EDrawMode { 纯色模式, 纹理模式, 线框模式};



#include <windows.h>
#include <ppl.h>

////===== 公用动画参数结构 =====
struct STRUCT_Ani
{
	EAniType aniType;
	bool isDoAni;  //是否播放动画
	int duration; //动画周期时长，毫秒
	int doCount;  //循环次数，0表示无限
	bool isReverse;  //完成后是否反转播放

	
	int doneCount; //已执行次数
	UINT startTick; //开始时的计时
};


#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <windows.h>

#include <d3d9.h>
#include <d3dx9.h>
#include <assert.h>



//===== 纹理材质顶点结构 =====
struct CUSTOMVERTEX_T
{
    D3DXVECTOR3 position; 
	D3DXVECTOR3 normal; 
    FLOAT u, v;
};
#define D3DFVF_CUSTOMVERTEX_T (D3DFVF_XYZ | D3DFVF_NORMAL| D3DFVF_TEX1)

////===== 色彩顶点结构 ===== 弃用暂留
struct CUSTOMVERTEX_C
{
    D3DXVECTOR3 position;
	DWORD color;
};
#define D3DFVF_CUSTOMVERTEX_C (D3DFVF_XYZ | D3DFVF_DIFFUSE)

////===== 色彩法线顶点结构 ===== 弃用暂留
struct CUSTOMVERTEX_CN
{
    D3DXVECTOR3 position;
	D3DXVECTOR3 normal; 
	D3DXCOLOR color;
};
#define D3DFVF_CUSTOMVERTEX_CN (D3DFVF_XYZ | D3DFVF_NORMAL| D3DFVF_DIFFUSE)

////===== 法线顶点结构 =====
struct CUSTOMVERTEX_N
{
    D3DXVECTOR3 position;
	D3DXVECTOR3 normal; 
};
#define D3DFVF_CUSTOMVERTEX_N (D3DFVF_XYZ | D3DFVF_NORMAL)

////===== 相机参数结构 =====
struct STRUCT_Camera
{
	D3DXVECTOR3 cameraPosition;
	D3DXVECTOR3 cameraLookat;
	D3DXVECTOR3 cameraDirection;
	D3DXVECTOR3 cameraUp;

	float FieldOfView;
	float Near;
	float Far;
};
//===== 光源结构 =====
struct STRUCT_Light
{
	bool isEnable;
	D3DLIGHT9 light;
};




#include "Helper.h"
#include "Global.h"
#include "Para.h"
#include "RendererManager.h"
#include "Renderer.h"
#include "TriangleRenderer.h"

#include "BasicModel.h"
#include "MapTile.h"
#include "Pline.h"
#include "PSymbol.h"
#include "PCustom.h"
#include "PPolyCol.h"
#include "PArea.h"
#include "PPanel.h"
#include "PText.h"
#include "Camera.h"
#include "XModel.h"

#define IFC(x) { hr = (x); if (FAILED(hr)) goto Cleanup; }
#define IFCOOM(x) { if ((x) == NULL) { hr = E_OUTOFMEMORY; IFC(hr); } }
#define SAFE_RELEASE(x) { if (x) { x->Release(); x = NULL; } }



typedef pair<int,CBasicModel*> EntryMap;
typedef pair<EModelType,pair<int,CBasicModel*>> EntryModelSort;
typedef pair<int,LPDIRECT3DTEXTURE9> EntryTexture;
typedef pair<int,ID3DXMesh*> EntryGeometry;
typedef pair<int,CXModel*> EntryXModel;






////===== 折线参数结构 =====
struct STRUCT_Line
{
	bool isReceivePick;
	int pickFlagId;
	int deepOrd;
	float thickness;
	D3DMATERIAL9 material;
	DWORD arrowColor;
    float arrowSize;
	bool isInverse;
	STRUCT_Ani aniDraw;
	STRUCT_Ani aniFlow;
	STRUCT_Ani aniTwinkle;
	D3DXVECTOR3 axis;
	float angle;
	int radCount;
};
////===== 区域参数结构 =====
struct STRUCT_Area
{
	bool isReceivePick;
	int pickFlagId;
	int deepOrd;
	D3DMATERIAL9 material;
	STRUCT_Ani aniShow;
	D3DXVECTOR3 axis;
	float angle;
};

////===== 图元参数结构 同时做为文字面板使用=====
struct STRUCT_Symbol
{
	bool isReceivePick;
	int pickFlagId;
	int deepOrd;
	int rootid;
	int parentid;
	int texturekey;
    float scaleX;
    float scaleY;
    float scaleZ;
	bool isH;
	D3DMATERIAL9 material;
	bool isUseColor;
	STRUCT_Ani aniTwinkle;
	STRUCT_Ani aniShow;
	STRUCT_Ani aniScale;
	bool isUseXModel;
    int XMKey;
	float XMScaleAddition;
	D3DXVECTOR3 axis;
	float angle;

};
////===== 数据对象参数结构 =====
struct STRUCT_PolyCol
{
	bool isReceivePick;
	int pickFlagId;
	int deepOrd;
	int rootid;
	int parentid;
	D3DMATERIAL9 material;
	float sizex;
	float sizey;
	float height;
	int geokey;
	STRUCT_Ani aniScale;
	STRUCT_Ani aniRotation;
	D3DXVECTOR3 axis;
	float angle;
};


////===== 公用几何体参数结构 =====
struct STRUCT_Geometry
{
	EGeometryType geoType;
	float pf1;
	float pf2;
	float pf3;
	int pi1;
	int pi2;
};


////===== 自定义模型参数结构 =====
struct STRUCT_Custom
{
	bool isReceivePick;
	int pickFlagId;
	int deepOrd;
	int rootid;
	int parentid;
    float scaleX;
    float scaleY;
    float scaleZ;
	D3DMATERIAL9 material;
	D3DXVECTOR3 axis;
	float angle;
	int texturekey;
	EDrawMode drawMode;
};
