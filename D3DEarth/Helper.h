#pragma once
#include <string>
using namespace std;


static class CHelper
{
public:
	CHelper(void);
	~CHelper(void);

	static D3DXMATRIXA16 getMatrixP2P(D3DXVECTOR3 pstart, D3DXVECTOR3 pend);
	static float getAngle(D3DXVECTOR3 v1, D3DXVECTOR3 v2);

	static wstring ToQuadKey(int level,int longitude, int latitude);

	static D3DXVECTOR3* JWHToPoint(D3DXVECTOR3* point, D3DXVECTOR3* jwh, float Radius);
    static D3DXVECTOR3* PointToJWH(D3DXVECTOR3* jwh, D3DXVECTOR3* point, float Radius);
        
	static POINT GetProjectPoint2D(D3DXVECTOR3 vec, D3DXMATRIX viewMatrix, D3DXMATRIX projMatrix, int Width, int Height);


};

