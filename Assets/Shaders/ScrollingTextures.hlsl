void GetScrollingUVs_float(in float Time, in float Speed, in float2 UV, out float2 ScrollingUV1, out float2 ScrollingUV2)
{
	ScrollingUV1 = UV;
	ScrollingUV2 = UV *0.5;
	
	ScrollingUV1.x += Time * Speed;
	ScrollingUV1.y += Time * Speed *0.1324862;
	
	ScrollingUV2.x -= Time * Speed * 0.36464521;
	ScrollingUV2.y -= Time * Speed *0.056784121;
}