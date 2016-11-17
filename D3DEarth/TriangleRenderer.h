#pragma once


class CCamera;
class CMapTile;
class CBasicModel;

#include <map>
#include <vector>
#include <string>
using namespace std;


    ///<summary>经纬坐标结构</summary>
    struct GeoPoint
    {
		GeoPoint()
		{
            Longitude = 0;
            Latitude = 0;
		}
        GeoPoint(double latitude, double longitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }
        ///<summary>经度</summary>
        double Longitude;
        ///<summary>纬度</summary>
        double Latitude;
    };


////===== 地图设置参数结构 =====
struct STRUCT_EarthPara
{
	STRUCT_EarthPara()
	{
		mapType=卫星;
		isShowOverlay=false;
		background=D3DCOLOR_ARGB(255, 0, 0, 0);

		tileReadMode=内置瓦片服务;
		//tileFileOffsetLI=0; //瓦片偏移-层序
		//tileFileOffsetXI=0; //瓦片偏移-X经度方向序号
		//tileFileOffsetYI=0; //瓦片偏移-Y纬度方向序号
		isCover=false;

		SceneMode=地球;
		InputCoordinate=WGS84球面坐标;
		StartLocation=GeoPoint(0,0);
		EndLocation=GeoPoint(0,0);
		StartLayer=0;
		StartIdxX=0;
		StartIdxY=0;
		EndIdxX=0;
		EndIdxY=0;
		UnitLatLen=0;
		UnitLoneLen=0;

		ArrowIntelvar=100;

		isDepthStencil=false;
	}


	EMapType mapType;
	bool isShowOverlay;
	DWORD background;

	ETileReadMode tileReadMode; //瓦片读取模式，缺省google web方式，自定义文件读取方式, 自定义web方式
	//int tileFileOffsetLI; //瓦片偏移-层序
	//int tileFileOffsetXI; //瓦片偏移-X经度方向序号
	//int tileFileOffsetYI; //瓦片偏移-Y纬度方向序号
	bool isCover;

	///<summary>场景模式，因为精度关系，若有极小的图形对象则应使用局面平面才能正常绘制极小的图形</summary>
	ESceneMode SceneMode;

	///<summary>外部输入采用的坐标系</summary>
	EInputCoordinate InputCoordinate;

	///<summary>局部平面场景下的起始坐标</summary>
	GeoPoint StartLocation;
	///<summary>局部平面场景下的结束坐标</summary>
	GeoPoint EndLocation;

	///<summary>局部平面场景下的有效起始层数</summary>
	int StartLayer;
	int StartIdxX;
	int StartIdxY;
	int EndIdxX;
	int EndIdxY;
	double UnitLatLen;
	double UnitLoneLen;
	double AdjustAspect; 
	double ArrowIntelvar; //潮流箭头间隔

	bool isDepthStencil;  //是否启用深度测试
};


//typedef pair<int,float> VECPAIR;  //模型纬度排序用
typedef pair<int,int> VECPAIR;  //模型纬度排序用
typedef pair<int,UINT> BUFFERPAIR;  //瓦片最近使用时间排序用

class CTriangleRenderer : public CRenderer
{
public:
    static HRESULT Create(IDirect3D9 *pD3D, IDirect3D9Ex *pD3DEx, HWND hwnd, UINT uAdapter, CRenderer **ppRenderer,UINT uWidth,UINT uHeight);
    ~CTriangleRenderer();

    HRESULT Render();
	
	CCamera *camera;
	void ChangeCameraAspect(float newaspect);
	void ChangeCameraPara(void* para,bool isAni,int duration);
	void ChangeLightPara(int lightNum, void* para);
	void ChangeAmbientLight(UINT color);
	int BeginTransfer();
	void EndTransfer();
	void AddMapTile(int id, int zlayer, int idxx,int idxy, bool isShowTerrain, int terrainSpan, void* pHigh);
	void LoadTile(); //初始化并载入瓦片资源

	int BeginTransferModel();
	void EndTransferModel();
	//void LoadModel(); //初始化并载入瓦片资源
	void ChangeProperty(EModelType modeltype,EPropertyType propertytype,int id ,int subid,void* para,int count,void* para2, int count2);
	void AddModel(EModelType modeltype,int id, void* para, void* pmesh, int mcount , void* ptexture, int tcount); //增加模型
	void AddCustomModel(int id, void* para, void* plocation, void* pvertices,void* pnormal, int vcount, void* pindex, int icount, void* puv, int uvcount, void* ptexture);

	bool isCancleTile; //用于提前中断瓦片载入线程
	int statusTile; //瓦片数据状态 0: 空闲  1: 传数据和计算中 2: 可更新
	int statusModel;//模型数据状态 0: 空闲  1: 传数据和计算中 2: 可更新


	//map<int,CBasicModel*> linemodels;  //绘制中的model
	//map<int,CBasicModel*> newlinemodels;  //models集合的buffer
	//
	//map<int,CBasicModel*> symbolmodels;  //绘制中的model
	//map<int,CBasicModel*> newsymbolmodels;  //models集合的buffer

	//map<int,CBasicModel*> textmodels;  //绘制中的model
	//map<int,CBasicModel*> newtextmodels;  //models集合的buffer

	//vector<VECPAIR> vecPolycol;
	//map<int,CBasicModel*> polycolmodels;  //绘制中的model
	//map<int,CBasicModel*> newpolycolmodels;  //models集合的buffer

	//map<int,CBasicModel*> areamodels;  //绘制中的model
	//map<int,CBasicModel*> newareamodels;  //models集合的buffer


	//map<int,CBasicModel*>* models[5];
	//map<int,CBasicModel*>* newmodels[5];

	vector<VECPAIR> vecModel;
	map<int,CBasicModel*> models;
	map<int,CBasicModel*> newmodels;


	int Pick_Model(POINT ptCursor);// 拾取模型
	int Pick_Model(POINT ptCursor,int flagid);// 拾取模型

	STRUCT_EarthPara earthpara; //地图的显示控制参数结构

	CPara para;

	wstring debuginfo;

	D3DMATERIAL9 basicmtrl;

protected:
    HRESULT Init(IDirect3D9 *pD3D, IDirect3D9Ex *pD3DEx, HWND hwnd, UINT uAdapter,UINT uWidth,UINT uHeight);

private:
    CTriangleRenderer();
	

	void UpdateMaps();
	void UpdateModels();

    IDirect3DVertexBuffer9 *m_pd3dVB;  //未使用

	map<int,CBasicModel*> maps;  //maptile集合
	map<int,CBasicModel*> newmaps;  //maptile集合的buffer

	map<int,CBasicModel*> buffermaps;  //maptile的buffer集合
		

	//光照相关
	STRUCT_Light lights[6];

	

	
};
