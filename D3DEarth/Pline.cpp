#include "StdAfx.h"
#include "xnamath.h"
#include <cmath>
#include "Parrow.h"


CPline::CPline(void)
{
	progress=1;
	radCount=4;
}


CPline::~CPline(void)
{
	delete vertices;
	delete dirs;
	delete lens;
	delete points;
	delete idxes;
}



HRESULT
CPline::Create(IDirect3DDevice9 *m_pd3dDevice, CBasicModel **ppModel , int id,void* ppara, void* pdots, int pointcount, CTriangleRenderer* tri)
{
	HRESULT hr = S_OK;

	CPline *pModel = new CPline();
    IFCOOM(pModel);
	pModel->myTri=tri;

	pModel->id=id;
    IFC(pModel->Init(m_pd3dDevice,ppara,pdots,pointcount));  //直接创建

    *ppModel = pModel;
    pModel = NULL;

Cleanup:
    delete pModel;

    return hr;

}

HRESULT
CPline::Render(IDirect3DDevice9 *m_pd3dDevice)
{
	HRESULT hr = S_OK;

	
 //   //设置灯光
 //   D3DXVECTOR3 vecDir;
 //   D3DLIGHT9 light;
 //   ZeroMemory( &light, sizeof(D3DLIGHT9) );
 //   light.Type       = D3DLIGHT_DIRECTIONAL;
 //   light.Diffuse.r  = 1.0f;
 //   light.Diffuse.g  = 1.0f;
 //   light.Diffuse.b  = 1.0f;
	//vecDir = D3DXVECTOR3(0.0f, 0.0f, -1.0f);
 //  D3DXVec3Normalize( (D3DXVECTOR3*)&light.Direction, &vecDir );
 //   light.Range       = 1000.0f;
 //   m_pd3dDevice->SetLight( 0, &light );
 //   m_pd3dDevice->LightEnable( 0, TRUE );
	//m_pd3dDevice->LightEnable( 2, false );
 //   m_pd3dDevice->SetRenderState( D3DRS_LIGHTING, TRUE );

 //   //设置环境光
 //   m_pd3dDevice->SetRenderState( D3DRS_AMBIENT, 0xff505050);


    m_pd3dDevice->SetRenderState(D3DRS_ALPHABLENDENABLE,true);
	m_pd3dDevice->SetRenderState(D3DRS_SRCBLEND,D3DBLEND_SRCALPHA);
	m_pd3dDevice->SetRenderState(D3DRS_DESTBLEND,D3DBLEND_INVSRCALPHA);
	m_pd3dDevice->SetRenderState( D3DRS_CULLMODE, D3DCULL_NONE );

	m_pd3dDevice->SetRenderState(D3DRS_ZWRITEENABLE,mtrl.Diffuse.a==1); //zh深度设置, 若为半透，需设为false



	//CGlobal::pCurRender->m_pd3dDevice->SetRenderState( D3DRS_CULLMODE, D3DCULL_NONE );

	IFC(m_pd3dDevice->SetTransform(D3DTS_WORLD, &world));

	if (aniDraw.isDoAni) //若执行绘制动画，实际填充顶点buffer
	{
		progress=(float)(GetTickCount()-aniDraw.startTick)/aniDraw.duration;
		if (aniDraw.aniType==擦除)
			progress=1-progress;
		if (progress<=0 && aniDraw.aniType==擦除)
			aniDraw.isDoAni=false;
		else if (progress>=1 && aniDraw.aniType==绘制)
			aniDraw.isDoAni=false;
		else
		{
			int passcount=0;
			D3DXVECTOR3 pos;
			//D3DXVECTOR3 posB;
			//D3DXVECTOR3 posC;
			//D3DXVECTOR3 posD;
			getPosition(progress,&passcount,&pos);
			buildVerticBuffer(passcount,pos);
		}
	}
	else if (progress<1)  //部分绘制
	{
			int passcount=0;
			D3DXVECTOR3 pos;
			//D3DXVECTOR3 posB;
			//D3DXVECTOR3 posC;
			//D3DXVECTOR3 posD;
			getPosition(progress,&passcount,&pos);
			buildVerticBuffer(passcount,pos);
	}

	if (aniTwinkle.isDoAni) //闪烁动画
	{
		float alpha=1;
		alpha=(float)((GetTickCount()-aniTwinkle.startTick)%aniTwinkle.duration)/aniTwinkle.duration;
		aniTwinkle.doneCount=(GetTickCount()-aniTwinkle.startTick)/aniTwinkle.duration;
		if (aniTwinkle.doCount==0 || aniTwinkle.doneCount<aniTwinkle.doCount)
		{
			if (aniTwinkle.isReverse && aniTwinkle.doneCount%2==1)
				alpha=1-alpha;
			alpha+=0.1;
		}
		else
		{
			aniTwinkle.doCount=0;
			aniTwinkle.isDoAni=false;
		}

		mtrl.Diffuse.a = mtrl.Ambient.a = alpha;//0.5f;
	}
	else
		mtrl.Diffuse.a = mtrl.Ambient.a = savealpha;//0.5f;




    //m_pd3dDevice->SetRenderState( D3DRS_LIGHTING, TRUE );
    //m_pd3dDevice->SetRenderState( D3DRS_AMBIENT, 0xffffffff);


    
    m_pd3dDevice->SetMaterial( &mtrl );

	if (aniDraw.isDoAni || progress<1) //绘制动画采用顶点buffer
	{
		IFC(m_pd3dDevice->SetStreamSource(0, m_pd3dVB, 0, sizeof(CUSTOMVERTEX_N)));
		IFC(m_pd3dDevice->SetFVF(D3DFVF_CUSTOMVERTEX_N));
		//IFC(m_pd3dDevice->DrawPrimitive(D3DPT_TRIANGLESTRIP, 0, PrimCount));   
		IFC(m_pd3dDevice->SetIndices(m_pd3dIB));
		IFC(m_pd3dDevice->DrawIndexedPrimitive(D3DPT_TRIANGLELIST,0,0,NumVertices,0,PrimCount));   
	}
	else if (aniDraw.aniType!=擦除)  //无绘制动画时，直接采用mesh
	{
		if (mesh!=NULL)
			mesh->DrawSubset(0);
	}

	if (aniFlow.isDoAni)  //是否有潮流动画
		for	(auto ent=submodels.begin();ent!=submodels.end();ent++)
		{
			ent->second->Render(m_pd3dDevice);
		}

Cleanup:
    return hr;

}

HRESULT
CPline::Init(IDirect3DDevice9 *m_pd3dDevice,void* ppara, void* pdots, int pointcount)
{
	HRESULT hr = S_OK;

	STRUCT_Line* para=(STRUCT_Line*)ppara;
	this->radCount=para->radCount;
	this->isReceivePick=para->isReceivePick;
	this->pickFlagId=para->pickFlagId;
	this->deepOrd=para->deepOrd;
	//this->color=para->color;
	memcpy(&mtrl,&para->material,sizeof(mtrl));
	this->thickness=para->thickness;
	this->arrowColor=para->arrowColor;
	this->arrowSize=para->arrowSize;
	this->isInverse=para->isInverse;
	aniDraw=para->aniDraw;
	aniFlow=para->aniFlow;
	aniTwinkle=para->aniTwinkle;

	this->axis=para->axis;
	this->angle=para->angle;

	pointCount=pointcount;

	D3DXVECTOR3* pd=(D3DXVECTOR3*)pdots;
	points=new D3DXVECTOR3[pointcount];
	memcpy(points,pd,sizeof(pd[0])*pointcount); //复制点集


	//============== 初始化计算参数
	lenth=0;  //线路长度
	dirs = new D3DXVECTOR3[pointCount - 1];
	lens = new float[pointCount - 1];
	for (int i = 0; i < pointCount - 1; i++)
	{
		dirs[i] = points[i + 1] - points[i];
		lens[i] = D3DXVec3Length(&dirs[i]);
		D3DXVec3Normalize(&dirs[i],&dirs[i]);
		lenth += lens[i];
	}
	
	buildMesh();

	if (aniDraw.isDoAni) //若要执行绘制动画，初始动画起始时间
		aniDraw.startTick=GetTickCount();
	if (aniTwinkle.isDoAni) //若要执行闪烁动画，初始动画起始时间
		aniTwinkle.startTick=GetTickCount();


	//////设置材质
 //   ZeroMemory( &mtrl, sizeof(D3DMATERIAL9) );
	//D3DXCOLOR c=D3DXCOLOR(color);

	//mtrl.Diffuse.r = mtrl.Ambient.r = c.r;//1.0f;
 //   mtrl.Diffuse.g = mtrl.Ambient.g =c.g;// 0.0f;
 //   mtrl.Diffuse.b = mtrl.Ambient.b =c.b;// 0.0f;
 //   mtrl.Diffuse.a = mtrl.Ambient.a =c.a;// 1.0f;
	//savealpha=c.a;
	savealpha=mtrl.Ambient.a;


	////===== 顶点缓冲	
	//IFC(m_pd3dDevice->CreateVertexBuffer(NumVertices*sizeof(CUSTOMVERTEX_C), 0, D3DFVF_CUSTOMVERTEX_C, D3DPOOL_DEFAULT, &m_pd3dVB, NULL)); 
	//void *pVertices;
	//IFC(m_pd3dVB->Lock(0, NumVertices*sizeof(CUSTOMVERTEX_C), &pVertices, 0));
	//memcpy(pVertices, vertices, NumVertices*sizeof(CUSTOMVERTEX_C));
	//m_pd3dVB->Unlock();
	
	//==============
	showFlow(aniFlow.isDoAni);

Cleanup:

    return hr;

}


void
CPline::showFlow(bool isshow)
{
	if (isshow && (submodels.size()==0))
	{
		int tmp=lenth/myTri->earthpara.ArrowIntelvar+1;
		for	(int i=0;i<tmp;i++)
		{
			CBasicModel* pmodel;
			CParrow::Create(myTri->m_pd3dDevice,&pmodel,i,this,myTri, i, tmp);
			submodels.insert(EntryMap(i,pmodel));
		}
	}
}

void
CPline::SetEffect(IDirect3DDevice9 *m_pd3dDevice,CTriangleRenderer* myTri)
{


}

void
CPline::ChangeProperty(EModelType modeltype, EPropertyType propertytype,void* para,int count)
{
	if (modeltype==折线)
	{
		if (propertytype==材质)
		{
			//memcpy(&color,(DWORD*)para,sizeof(color));
			//changeColor(color);
			memcpy(&mtrl,(D3DMATERIAL9*)para,sizeof(mtrl));
		}
		else if (propertytype==宽度)
		{
			memcpy(&thickness,(DWORD*)para,sizeof(thickness));
			buildMesh();
		}
		else if (propertytype==位置)
		{
			pointCount=count;
			D3DXVECTOR3* pd=(D3DXVECTOR3*)para;
			points=new D3DXVECTOR3[count];
			memcpy(points,pd,sizeof(pd[0])*count); //复制点集
			//============== 初始化计算参数
			lenth=0;  //线路长度
			dirs = new D3DXVECTOR3[pointCount - 1];
			lens = new float[pointCount - 1];
			for (int i = 0; i < pointCount - 1; i++)
			{
				dirs[i] = points[i + 1] - points[i];
				lens[i] = D3DXVec3Length(&dirs[i]);
				D3DXVec3Normalize(&dirs[i],&dirs[i]);
				lenth += lens[i];
			}
			buildMesh();
		}
		else if (propertytype==进度)
		{
			memcpy(&progress,(float*)para,sizeof(progress));
			//buildMesh();
		}

		else if (propertytype==动画)
		{
			STRUCT_Ani* lpara=(STRUCT_Ani*)para;
			if (lpara->aniType==擦除 || lpara->aniType==绘制)
			{
				memcpy(&aniDraw,para,sizeof(aniDraw));
				aniDraw.startTick=GetTickCount();
			}
			else if (lpara->aniType==闪烁)
			{
				memcpy(&aniTwinkle,para,sizeof(aniTwinkle));
				aniTwinkle.startTick=GetTickCount();
			}
		}
	}
	else if (modeltype==潮流)
	{
		if (propertytype==颜色)  //潮流箭头暂不实现改色，因为少有用，且本来是顶点色
		{
			//memcpy(&arrowColor,(DWORD*)para,sizeof(color));
			//for	(auto iter=submodels.begin();iter!=submodels.end();iter++)
			//	iter->second->changeColor(arrowColor);
		}
		else if (propertytype==动画) //是否显示潮流
		{
			memcpy(&aniFlow,(bool*)para,sizeof(aniFlow));
			showFlow(aniFlow.isDoAni);
		}
		else if (propertytype==大小) //潮流箭头大小
		{
			memcpy(&arrowSize,(float*)para,sizeof(arrowSize));
			for	(auto iter=submodels.begin();iter!=submodels.end();iter++)
				((CParrow*)iter->second)->initBaseWorld();
		}
		
	}
}

//--------------------------------------------------------------------
// 根据点集，创建mesh
//--------------------------------------------------------------------
void 
CPline::buildMesh()
{
	if (pointCount==2) //若点距过近，不绘制
	{
		if (D3DXVec3Length(&(points[1]-points[0]))<0.00001)
		{
			if (mesh!=NULL)
				mesh->Release();
			mesh=NULL;
			return;
		}
	}


	//D3DXVECTOR3* expandpoints(nullptr);expandpoints=new D3DXVECTOR3[pointCount][64];
	D3DXVECTOR3 **expandpoints;
	expandpoints = new D3DXVECTOR3*[pointCount]; //行 //申请行的空间
	//每行的列申请空间
	for(int i=0; i<pointCount;i++)
	{
		expandpoints[i] = new D3DXVECTOR3 [radCount];
	}
	

	for (int i = 0; i < pointCount; i++)
	{
		D3DXVECTOR3 vecdir;  //1->2连线方向
		D3DXVECTOR3 veccross;  //1->2交叉线方向, 取水平和垂线之一
		D3DXVECTOR3 vec0;  //第0扩展点方向
		D3DXVECTOR3 vecN;  //第N扩展点方向
		D3DXVECTOR3 vecup;  //向上方向，分地球或平面两种取法
		D3DXVECTOR3 vechor; //水平方向，若连线方向与vecup一样，则取此方向参与运算
		D3DXVECTOR3 vectmp1,vectmp2, vectmp3, vectmp4, vectmp5;


		if (i==0) 
		{
			vecdir=(*(points+i+1)-*(points+i)); //连线方向
			D3DXVec3Normalize(&vecdir,&vecdir);
			if (myTri->earthpara.SceneMode==地球) //求向上和水平方向
			{
				D3DXVec3Normalize(&vecup,&(points[i]));
				vectmp1=D3DXVECTOR3(0,1,0);  //假定不会有取向南极点
				D3DXVec3Cross(&vechor,&vecdir,&vectmp1);
				D3DXVec3Normalize(&vechor,&vechor);
			}
			else
			{
				vecup=D3DXVECTOR3(0,0,1);
				vechor=D3DXVECTOR3(0,1,0);
			}
			veccross= vecup==vecdir? vechor:vecup;  //判断使用的交叉线方向
			D3DXVec3Cross(&vec0,&veccross,&vecdir); D3DXVec3Normalize(&vec0,&vec0);  //获得第一个点方向

			D3DXMATRIXA16 rotation;
			for (int j=0; j<radCount;j ++)
			{
				D3DXMatrixRotationAxis(&rotation, &vecdir, D3DX_PI*2*j/radCount); 
				D3DXVec3TransformNormal(&vecN,&vec0,&rotation);  //获得第N个点单位向量
				expandpoints[i][j] = points[i] + thickness / 2 * vecN;
			}
		}
		else  //后续点用平面相交测点
		{
			vecdir=(*(points+i)-*(points+i-1)); //连线方向
            D3DXVec3Normalize(&vecdir,&vecdir);
			for (int j=0; j<radCount;j ++)
			{
				expandpoints[i][j] = expandpoints[i-1][j]+vecdir;
			}

			D3DXPLANE plane;
			if (i!=pointCount-1) //中间点，采用椭圆面相交来确定扩展点
			{
				//求椭圆平面
				vectmp1=(*(points+i+1)-*(points+i));D3DXVec3Normalize(&vectmp1,&vectmp1);  //后连线
				vectmp2=(*(points+i-1)-*(points+i));D3DXVec3Normalize(&vectmp2,&vectmp2);  //前连线
				float dotp=D3DXVec3Dot(&vectmp1,&vectmp2);				dotp=dotp>1?1:dotp;				dotp=dotp<-1?-1:dotp;
				float angle=acos(dotp);  //夹角
				D3DXVec3Cross(&vectmp3,&vectmp1,&vectmp2);  //轴，在椭圆面上
				D3DXMATRIXA16 rotatmp;
				D3DXMatrixRotationAxis(&rotatmp, &vectmp3, angle/2);   
				D3DXVec3TransformNormal(&vectmp4,&vectmp1,&rotatmp); //旋转到夹角中央，即椭圆面上
				D3DXVec3Cross(&vectmp5,&vectmp3,&vectmp4);  //椭圆面法线
				if (angle<3.1415*10/180)  //若角小于10度，转折距离太大，改以折裂形式
					D3DXPlaneFromPointNormal(&plane,(points+i),&vectmp4);
				else if (angle>3.14) //若近似直线也直接用截面
					D3DXPlaneFromPointNormal(&plane,(points+i),&vecdir);
				else
					D3DXPlaneFromPointNormal(&plane,(points+i),&vectmp5);

			}
			else  //最终点
			{
				//求圆平面
				D3DXPlaneFromPointNormal(&plane,(points+i),&vecdir);
			}

			for (int j=0; j<radCount;j ++)
			{
				D3DXPlaneIntersectLine(&vectmp1,&plane,&expandpoints[i-1][j],&expandpoints[i][j]);
				expandpoints[i][j]=vectmp1;
			}

		}
	}
	
	vertices=new CUSTOMVERTEX_N[pointCount * radCount];
	for (int i = 0; i < pointCount; i++)
	{
		for (int j=0; j<radCount; j++)
		{
			vertices[i*radCount+j].position=expandpoints[i][j];
			//vertices[i*radCount+j].normal=  (expandpoints[i][j]-*(points+i));
			D3DXVec3Normalize(&vertices[i*radCount+j].normal, &(expandpoints[i][j]-*(points+i)));
		}

	}

	operate=1;
	status=1;

	
	NumVertices=pointCount * radCount;
	PrimCount=(pointCount-1) * 2* radCount;



	// 建立mesh
	if (mesh!=NULL)
		mesh->Release();

	D3DXCreateMeshFVF(PrimCount,NumVertices,D3DXMESH_DYNAMIC, D3DFVF_CUSTOMVERTEX_N,myTri->m_pd3dDevice, &mesh);
	
	CUSTOMVERTEX_N* v = 0;
	mesh->LockVertexBuffer(0, (void**)&v);
	for (int i=0;i<NumVertices;i++)
		v[i]=vertices[i];
	mesh->UnlockVertexBuffer();

 
	idxes=new WORD[PrimCount*3];
	for (int i=0;i<pointCount-1;i++)
	{
		for (int j=0; j<radCount;j++)
		{
			if (j<radCount-1)
			{
				idxes[i*6*radCount+j*6+0]=i*radCount+j;
				idxes[i*6*radCount+j*6+2]=i*radCount+j+radCount;
				idxes[i*6*radCount+j*6+1]=i*radCount+j+1;
				idxes[i*6*radCount+j*6+3]=i*radCount+j+radCount;
				idxes[i*6*radCount+j*6+5]=i*radCount+j+radCount+1;
				idxes[i*6*radCount+j*6+4]=i*radCount+j+1;
			}
			else
			{
				idxes[i*6*radCount+j*6+0]=i*radCount+radCount-1;
				idxes[i*6*radCount+j*6+2]=i*radCount+radCount+radCount-1;
				idxes[i*6*radCount+j*6+1]=i*radCount+0;
				idxes[i*6*radCount+j*6+3]=i*radCount+radCount+radCount-1;
				idxes[i*6*radCount+j*6+5]=i*radCount+radCount;
				idxes[i*6*radCount+j*6+4]=i*radCount+0;
			}
		}

	}

	WORD* idx = 0;
	mesh->LockIndexBuffer(0, (void**)&idx);
	for (int i=0; i<PrimCount*3;i++)
		idx[i]=idxes[i];
	mesh->UnlockIndexBuffer();
	//优化
	vector<DWORD> adjacencyBuffer(mesh->GetNumFaces() * 3);
	mesh->GenerateAdjacency(0.0f, &adjacencyBuffer[0]);
	mesh->OptimizeInplace(D3DXMESHOPT_ATTRSORT|D3DXMESHOPT_COMPACT|D3DXMESHOPT_VERTEXCACHE, &adjacencyBuffer[0],0, 0, 0);


	//===== 索引缓冲
	myTri->m_pd3dDevice->CreateIndexBuffer(PrimCount*3*sizeof(WORD), 0, D3DFMT_INDEX16 , D3DPOOL_DEFAULT, &m_pd3dIB, NULL); 
	void *pIndexes;
	(m_pd3dIB->Lock(0, PrimCount*3*sizeof(WORD), &pIndexes, 0));
	memcpy(pIndexes, idx, PrimCount*3*sizeof(WORD));
	m_pd3dIB->Unlock();


	//delete vertices;
	//vertices=nullptr;
	//delete idxes;
	//idxes=nullptr;

	for(int i=0;i<pointCount;i++)
	{
		delete [] expandpoints[i];
		expandpoints[i]=nullptr;
	}
	delete [] expandpoints;
	expandpoints=nullptr;


}


//--------------------------------------------------------------------
// 根据progress，计算动画走到的位置
//--------------------------------------------------------------------
void CPline::getPosition(float progress, int* passcount, D3DXVECTOR3* pos)
{
	float len = 0;
	int count=0;

	for (int i = 0; i < pointCount - 1; i++)
	{
		float tmp = len +lens[i];
		if (tmp /lenth > progress)
		{
			*passcount=i+1;
			float tmp2 = progress * lenth - len;
			*pos = points[i] + dirs[i] * tmp2;

			//D3DXVECTOR3 vexpand;
			//D3DXVec3Cross(&vexpand,&(*(points+i+1)-*(points+i)),points+i);
			//D3DXVec3Normalize(&vexpand,&vexpand);
			//*posA = pos + thickness / 2 * vexpand;
			//*posC = pos - thickness / 2 * vexpand;

			//D3DXVECTOR3 vecup;
			//D3DXVec3Normalize(&vecup,&pos);
			//*posB = pos + thickness / 2 * vecup;
			//*posD = pos - thickness / 2 * vecup;

			break;
		}
		len=tmp;
	}

}

//--------------------------------------------------------------------
// 创建绘制动画的顶点buffer
//--------------------------------------------------------------------
void CPline::buildVerticBuffer(int passcount,  D3DXVECTOR3 pos)
{

	NumVertices=(passcount+1) * radCount;
	PrimCount=passcount * radCount*2;

	CUSTOMVERTEX_N* pvtemp(nullptr);
	pvtemp=new CUSTOMVERTEX_N[NumVertices];

	memcpy(pvtemp,vertices,NumVertices*sizeof(CUSTOMVERTEX_N));


	D3DXPLANE plane;
	D3DXVECTOR3	vecdir=(pos-*(points+passcount)); //连线方向
    D3DXVec3Normalize(&vecdir,&vecdir);
	//求圆平面
	D3DXPlaneFromPointNormal(&plane,&pos,&vecdir);
			
	D3DXVECTOR3 vectmp1,vectmp2;
	for (int j=0; j<radCount;j ++)
	{
		vectmp2=(pvtemp[NumVertices-1-j-radCount].position)+vecdir;
		D3DXPlaneIntersectLine(&vectmp1,&plane,&pvtemp[NumVertices-1-j-radCount].position,&vectmp2);
	
		pvtemp[NumVertices-1-j].position=vectmp1;
		pvtemp[NumVertices-1-j].normal=vectmp1;
	}

	//pvtemp[NumVertices-1].position=posD;
	//pvtemp[NumVertices-1].normal=posD;
	//pvtemp[NumVertices-2].position=posC;
	//pvtemp[NumVertices-2].normal=posC;
	//pvtemp[NumVertices-3].position=posB;
	//pvtemp[NumVertices-3].normal=posB;
	//pvtemp[NumVertices-4].position=posA;
	//pvtemp[NumVertices-4].normal=posA;




	
	//===== 顶点缓冲	
	if (m_pd3dVB!=nullptr)
		m_pd3dVB->Release();//delete m_pd3dVB;
	myTri->m_pd3dDevice->CreateVertexBuffer(NumVertices*sizeof(CUSTOMVERTEX_N), 0, D3DFVF_CUSTOMVERTEX_N, D3DPOOL_DEFAULT, &m_pd3dVB, NULL); 
	void *pVertices;
	m_pd3dVB->Lock(0, NumVertices*sizeof(CUSTOMVERTEX_N), &pVertices, 0);
	memcpy(pVertices, pvtemp, NumVertices*sizeof(CUSTOMVERTEX_N));//复制固定点
	m_pd3dVB->Unlock();

	


	//===== 索引缓冲
	myTri->m_pd3dDevice->CreateIndexBuffer(PrimCount*3*sizeof(WORD), 0, D3DFMT_INDEX16 , D3DPOOL_DEFAULT, &m_pd3dIB, NULL); 
	void *pIndexes;
	(m_pd3dIB->Lock(0, PrimCount*3*sizeof(WORD), &pIndexes, 0));
	memcpy(pIndexes, idxes, PrimCount*3*sizeof(WORD));
	m_pd3dIB->Unlock();

	

	
	
	
	delete pvtemp;
}


