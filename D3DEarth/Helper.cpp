#include "StdAfx.h"
#include "Helper.h"


CHelper::CHelper(void)
{
}


CHelper::~CHelper(void)
{
}

D3DXMATRIXA16
CHelper::getMatrixP2P(D3DXVECTOR3 pstart, D3DXVECTOR3 pend)
{
	D3DXMATRIXA16 matrix;
	D3DXMatrixIdentity(&matrix);
                        
	float angle1 = atan(pstart.x / pstart.z);
	float angle2 = atan(pend.x / pend.z);
	if (angle2 != angle1)
		D3DXMatrixRotationY(&matrix,angle2-angle1);

	D3DXVECTOR3 pmid;
	D3DXVec3TransformCoord(&pmid,&pstart,&matrix);
	if (pmid != pend)  
	{
		D3DXVECTOR3 axis;
		D3DXVec3Cross(&axis,&pmid,&pend);
		D3DXVec3Normalize(&axis,&axis);

		float angle = getAngle(pmid,pend);

		D3DXMATRIXA16 tmpmatrix;
		D3DXMatrixRotationAxis(&tmpmatrix,&axis,angle);
		D3DXMatrixMultiply(&matrix,&matrix,&tmpmatrix);
	}
	return matrix;
}

float
CHelper::getAngle(D3DXVECTOR3 v1, D3DXVECTOR3 v2)
{
	return (float)acos(D3DXVec3Dot(&v1,&v2)/D3DXVec3Length(&v1)/D3DXVec3Length(&v2)) ;
}



wstring 
CHelper::ToQuadKey(int level,int longitude, int latitude) 
 { 
     unsigned long long quadkey = 0; 
     int mask = 1 << (level - 1); 

     for (int i = 0; i < level; i++) 
     { 
         quadkey <<= 2; 

         if ((longitude & mask) != 0) 
             quadkey |= 1; 

         if ((latitude & mask) != 0) 
             quadkey |= 2; 

         mask >>= 1; 
     } 

     std::wstring str; 

     for (int i = 0; i < level; i++) 
     { 
         str.insert(0, 1, (quadkey & 3) + '0'); 
         quadkey >>= 2; 
     } 
     return str; 
 }


D3DXVECTOR3*
CHelper::JWHToPoint( D3DXVECTOR3* point, D3DXVECTOR3* jwh, float Radius) //经纬高转换为坐标点
        {
            double x, y, z, r, r2;
            r = Radius + jwh->z;
            y = r * sin(jwh->y * D3DX_PI / 180);
            r2 = r * cos(jwh->y * D3DX_PI / 180);
            x = r2 * cos((-jwh->x + 180) * D3DX_PI / 180);
            z = r2 * sin((-jwh->x + 180) * D3DX_PI / 180);
			point->x=x; point->y=y; point->z=z;
            return point;
        }

D3DXVECTOR3*
CHelper::PointToJWH(D3DXVECTOR3* jwh, D3DXVECTOR3* point , float Radius)   //坐标点转换为经纬高
        {
            double j, w, h, r, r2;
            r =D3DXVec3Length(point);
            h = r - Radius;
            w = asin(point->y / r) * 180 / D3DX_PI;

            r2 = r * cos(w * D3DX_PI / 180);
			j = -acos(point->x / r2) * 180 / D3DX_PI + 180;
            if (point->z < 0) j = 360.0 - j;
			jwh->x=j; jwh->y=w; jwh->z=h;
            return jwh;
        }

//获取3D转2D屏幕坐标
POINT
CHelper::GetProjectPoint2D(D3DXVECTOR3 vec, D3DXMATRIX viewMatrix, D3DXMATRIX projMatrix, int Width, int Height)
{
	D3DXMATRIXA16 mat;
	D3DXMatrixIdentity(&mat);
	mat *= viewMatrix;
	mat*= projMatrix;

	D3DXVECTOR4 v4;
	D3DXVec3Transform(&v4,&vec,&mat);
	POINT pt;
	pt.x=(int)((v4.x / v4.w + 1) * (Width / 2));
	pt.y=(int)((1 - v4.y / v4.w) * (Height / 2));
	return pt;

}
