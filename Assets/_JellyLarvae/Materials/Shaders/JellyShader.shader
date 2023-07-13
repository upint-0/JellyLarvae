// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/JellyShader"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[ASEBegin]_MainTex("_MainTex", 2D) = "white" {}
		_JellyColor("JellyColor", Color) = (0.2723389,0.6792453,0.4855379,1)
		_JellyOutlineTint("JellyOutlineTint", Color) = (0.2988608,0.8679245,0.4786123,1)
		_JellyThreshold("JellyThreshold", Float) = 0.1
		_JellyOutlineThickness("JellyOutlineThickness", Float) = 0.15
		_Voronoi8512x512("Voronoi 8 - 512x512", 2D) = "white" {}
		_7524normal("7524-normal", 2D) = "white" {}
		_DistortAmount("DistortAmount", Float) = 0
		_DistortTiling("DistortTiling", Float) = 0
		[ASEEnd]_VoronoiTiling("VoronoiTiling", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }

		Cull Off
		HLSLINCLUDE
		#pragma target 2.0
		
		#pragma prefer_hlslcc gles
		#pragma exclude_renderers d3d11_9x 

		ENDHLSL

		
		Pass
		{
			Name "Unlit"
			

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZTest LEqual
			ZWrite Off
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			
			#define ASE_SRP_VERSION 999999

			
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA

			#define _SURFACE_TYPE_TRANSPARENT 1
			#define SHADERPASS_SPRITEUNLIT

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_TANGENT


			sampler2D _CameraSortingLayerTexture;
			sampler2D _7524normal;
			sampler2D _Voronoi8512x512;
			sampler2D _MainTex;
			CBUFFER_START( UnityPerMaterial )
			float4 _JellyColor;
			float4 _MainTex_ST;
			float4 _JellyOutlineTint;
			float _DistortTiling;
			float _DistortAmount;
			float _VoronoiTiling;
			float _JellyThreshold;
			float _JellyOutlineThickness;
			CBUFFER_END


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 uv0 : TEXCOORD0;
				float4 color : COLOR;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 texCoord0 : TEXCOORD0;
				float4 color : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			#if ETC1_EXTERNAL_ALPHA
				TEXTURE2D( _AlphaTex ); SAMPLER( sampler_AlphaTex );
				float _EnableAlphaTexture;
			#endif

			float4 _RendererColor;

			inline float4 ASE_ComputeGrabScreenPos( float4 pos )
			{
				#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
				#else
				float scale = 1.0;
				#endif
				float4 o = pos;
				o.y = pos.w * 0.5f;
				o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
				return o;
			}
			
			float3 PerturbNormal107_g4( float3 surf_pos, float3 surf_norm, float height, float scale )
			{
				// "Bump Mapping Unparametrized Surfaces on the GPU" by Morten S. Mikkelsen
				float3 vSigmaS = ddx( surf_pos );
				float3 vSigmaT = ddy( surf_pos );
				float3 vN = surf_norm;
				float3 vR1 = cross( vSigmaT , vN );
				float3 vR2 = cross( vN , vSigmaS );
				float fDet = dot( vSigmaS , vR1 );
				float dBs = ddx( height );
				float dBt = ddy( height );
				float3 vSurfGrad = scale * 0.05 * sign( fDet ) * ( dBs * vR1 + dBt * vR2 );
				return normalize ( abs( fDet ) * vN - vSurfGrad );
			}
			

			VertexOutput vert( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord2 = screenPos;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				o.ase_texcoord3.xyz = ase_worldPos;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.normal);
				o.ase_texcoord4.xyz = ase_worldNormal;
				float3 ase_worldTangent = TransformObjectToWorldDir(v.tangent.xyz);
				o.ase_texcoord5.xyz = ase_worldTangent;
				float ase_vertexTangentSign = v.tangent.w * unity_WorldTransformParams.w;
				float3 ase_worldBitangent = cross( ase_worldNormal, ase_worldTangent ) * ase_vertexTangentSign;
				o.ase_texcoord6.xyz = ase_worldBitangent;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.w = 0;
				o.ase_texcoord4.w = 0;
				o.ase_texcoord5.w = 0;
				o.ase_texcoord6.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.normal = v.normal;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.vertex.xyz );

				o.texCoord0 = v.uv0;
				o.color = v.color;
				o.clipPos = vertexInput.positionCS;

				return o;
			}

			half4 frag( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float4 screenPos = IN.ase_texcoord2;
				float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( screenPos );
				float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
				float3 ase_worldPos = IN.ase_texcoord3.xyz;
				float2 appendResult54 = (float2(ase_worldPos.x , ase_worldPos.y));
				float2 panner41 = ( _TimeParameters.x * float2( 0.01,-0.01 ) + ( appendResult54 * _DistortTiling ));
				float temp_output_64_0 = length( ( ( ase_worldPos - float3( 0.5,0.5,0 ) ) * float3( 2,2,0 ) ) );
				float3 surf_pos107_g4 = ase_worldPos;
				float3 ase_worldNormal = IN.ase_texcoord4.xyz;
				float3 surf_norm107_g4 = ase_worldNormal;
				float mulTime63 = _TimeParameters.x * -1.0;
				float smoothstepResult69 = smoothstep( 0.5 , 1.0 , ( ( sin( ( ( temp_output_64_0 + mulTime63 ) * 10.0 ) ) + 1.0 ) * 0.5 ));
				float height107_g4 = smoothstepResult69;
				float scale107_g4 = 1.0;
				float3 localPerturbNormal107_g4 = PerturbNormal107_g4( surf_pos107_g4 , surf_norm107_g4 , height107_g4 , scale107_g4 );
				float3 ase_worldTangent = IN.ase_texcoord5.xyz;
				float3 ase_worldBitangent = IN.ase_texcoord6.xyz;
				float3x3 ase_worldToTangent = float3x3(ase_worldTangent,ase_worldBitangent,ase_worldNormal);
				float3 worldToTangentDir42_g4 = mul( ase_worldToTangent, localPerturbNormal107_g4);
				float2 appendResult56 = (float2(ase_worldPos.x , ase_worldPos.y));
				float4 lerpResult32 = lerp( tex2D( _CameraSortingLayerTexture, ( float3( (ase_grabScreenPosNorm).xy ,  0.0 ) + float3( ( ( ( (tex2D( _7524normal, panner41 )).rg + -0.5 ) * 2.0 ) * _DistortAmount ) ,  0.0 ) + ( ( saturate( ( 1.0 - temp_output_64_0 ) ) * worldToTangentDir42_g4 ) * 0.5 ) ).xy ) , _JellyColor , tex2D( _Voronoi8512x512, ( appendResult56 * _VoronoiTiling ) ).r);
				float2 uv_MainTex = IN.texCoord0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 tex2DNode1 = tex2D( _MainTex, uv_MainTex );
				float temp_output_7_0 = step( _JellyThreshold , tex2DNode1.r );
				float JellyMask18 = temp_output_7_0;
				float OutlineMask19 = ( temp_output_7_0 - step( ( _JellyThreshold + _JellyOutlineThickness ) , tex2DNode1.r ) );
				
				float4 Color = ( ( lerpResult32 * ( JellyMask18 - OutlineMask19 ) ) + ( _JellyOutlineTint * OutlineMask19 ) );

				#if ETC1_EXTERNAL_ALPHA
					float4 alpha = SAMPLE_TEXTURE2D( _AlphaTex, sampler_AlphaTex, IN.texCoord0.xy );
					Color.a = lerp( Color.a, alpha.r, _EnableAlphaTexture );
				#endif

				Color *= IN.color;

				return Color;
			}

			ENDHLSL
		}
	}
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=18917
152;421;1196;848;4316.999;1322.529;1;True;False
Node;AmplifyShaderEditor.WorldPosInputsNode;76;-3682.999,-1024.529;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;61;-3460.447,-1080.602;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0.5,0.5,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;62;-3293.262,-1080.427;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;2,2,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleTimeNode;63;-3200.146,-963.2689;Inherit;False;1;0;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LengthOpNode;64;-3146.396,-1079.405;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;65;-3005.445,-1082.569;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;53;-2717.688,-799.1577;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;54;-2521.042,-773.0958;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;52;-2680.537,-491.2251;Inherit;False;Property;_DistortTiling;DistortTiling;9;0;Create;True;0;0;0;False;0;False;0;0.18;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-2869.376,-1082.447;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;43;-2476.442,-508.1309;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;50;-2464.286,-606.3779;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;10;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SinOpNode;67;-2738.375,-1081.447;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;68;-2612.814,-1080.629;Inherit;False;ConstantBiasScale;-1;;3;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;41;-2304.763,-606.1243;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.01,-0.01;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-1252.892,142.6655;Inherit;False;Property;_JellyThreshold;JellyThreshold;3;0;Create;True;0;0;0;False;0;False;0.1;0.21;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-1296.893,214.6657;Inherit;False;Property;_JellyOutlineThickness;JellyOutlineThickness;4;0;Create;True;0;0;0;False;0;False;0.15;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;69;-2355.25,-1081.211;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;70;-2998.47,-1175.741;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;34;-2133.077,-634.8072;Inherit;True;Property;_7524normal;7524-normal;6;0;Create;True;0;0;0;False;0;False;-1;0e0b983abd604284e9369872714a0c8e;0e0b983abd604284e9369872714a0c8e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;71;-2847.673,-1175.729;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;72;-2052.115,-1081.386;Inherit;False;Normal From Height;-1;;4;1942fe2c5f1a1f94881a33d532e4afeb;0;2;20;FLOAT;0;False;110;FLOAT;1;False;2;FLOAT3;40;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;13;-1066.891,195.6657;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-1391.421,-54.99947;Inherit;True;Property;_MainTex;_MainTex;0;0;Create;True;0;0;0;False;0;False;-1;None;a0116db4ca7623c49aa13fd019c89f89;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;36;-1841.291,-634.913;Inherit;False;True;True;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;75;-1723.902,-1047.091;Inherit;False;Constant;_Float0;Float 0;11;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;7;-917.8981,-48.03962;Inherit;False;2;0;FLOAT;0.1;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;73;-1735.61,-1177.151;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GrabScreenPosition;46;-1700.124,-826.9164;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;49;-1632.348,-629.4335;Inherit;False;ConstantBiasScale;-1;;1;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT2;0,0;False;1;FLOAT;-0.5;False;2;FLOAT;2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StepOpNode;8;-916.8981,49.96047;Inherit;False;2;0;FLOAT;0.11;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-1605.312,-484.467;Inherit;False;Property;_DistortAmount;DistortAmount;8;0;Create;True;0;0;0;False;0;False;0;0.08;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;55;-1563.835,-368.0648;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ComponentMaskNode;48;-1493.322,-826.1322;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-1564.299,-1138.231;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;9;-768.8981,-0.03954935;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-1395.312,-630.467;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;58;-1417.516,-209.982;Inherit;False;Property;_VoronoiTiling;VoronoiTiling;10;0;Create;True;0;0;0;False;0;False;1;0.025;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;56;-1367.189,-342.0029;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;18;-597.235,-53.53641;Inherit;False;JellyMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;57;-1236.47,-302.3157;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;39;-1261.312,-764.467;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;29;-1145.771,-937.8976;Inherit;True;Global;_CameraSortingLayerTexture;_CameraSortingLayerTexture;7;0;Create;True;0;0;0;False;0;False;None;;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RegisterLocalVarNode;19;-396.235,-4.536321;Inherit;False;OutlineMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;26;-1117.127,-604.7363;Inherit;True;Property;_Voronoi8512x512;Voronoi 8 - 512x512;5;0;Create;True;0;0;0;False;0;False;-1;d8f4a93761e43df4fa87ae74645fb19f;d8f4a93761e43df4fa87ae74645fb19f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;20;-591.1249,-386.1561;Inherit;False;18;JellyMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;2;-817.4654,-467.8048;Inherit;False;Property;_JellyColor;JellyColor;1;0;Create;True;0;0;0;False;0;False;0.2723389,0.6792453,0.4855379,1;0.2991722,0.764151,0.448146,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;31;-851.7709,-794.075;Inherit;True;Property;_TextureSample0;Texture Sample 0;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;24;-600.1249,-213.1561;Inherit;False;19;OutlineMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;25;-415.8442,-380.9885;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;14;-813.0818,-285.5612;Inherit;False;Property;_JellyOutlineTint;JellyOutlineTint;2;0;Create;True;0;0;0;False;0;False;0.2988608,0.8679245,0.4786123,1;0.6497418,0.8773585,0.7216209,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;32;-502.9786,-618.7057;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-283.1249,-280.1561;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-282.4247,-463.7561;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;60;-3943.342,-1084.404;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;22;-118.4249,-387.7561;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;42;-2701.218,-607.2432;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;63.35455,-386.7455;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;13;Custom/JellyShader;cf964e524c8e69742b1d21fbe2ebcc4a;True;Unlit;0;0;Unlit;3;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;2;5;False;-1;10;False;-1;3;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;False;0;Hidden/InternalErrorShader;0;0;Standard;1;Vertex Position;1;0;1;True;False;;False;0
WireConnection;61;0;76;0
WireConnection;62;0;61;0
WireConnection;64;0;62;0
WireConnection;65;0;64;0
WireConnection;65;1;63;0
WireConnection;54;0;53;1
WireConnection;54;1;53;2
WireConnection;66;0;65;0
WireConnection;50;0;54;0
WireConnection;50;1;52;0
WireConnection;67;0;66;0
WireConnection;68;3;67;0
WireConnection;41;0;50;0
WireConnection;41;1;43;0
WireConnection;69;0;68;0
WireConnection;70;0;64;0
WireConnection;34;1;41;0
WireConnection;71;0;70;0
WireConnection;72;20;69;0
WireConnection;13;0;10;0
WireConnection;13;1;11;0
WireConnection;36;0;34;0
WireConnection;7;0;10;0
WireConnection;7;1;1;1
WireConnection;73;0;71;0
WireConnection;73;1;72;40
WireConnection;49;3;36;0
WireConnection;8;0;13;0
WireConnection;8;1;1;1
WireConnection;48;0;46;0
WireConnection;74;0;73;0
WireConnection;74;1;75;0
WireConnection;9;0;7;0
WireConnection;9;1;8;0
WireConnection;38;0;49;0
WireConnection;38;1;40;0
WireConnection;56;0;55;1
WireConnection;56;1;55;2
WireConnection;18;0;7;0
WireConnection;57;0;56;0
WireConnection;57;1;58;0
WireConnection;39;0;48;0
WireConnection;39;1;38;0
WireConnection;39;2;74;0
WireConnection;19;0;9;0
WireConnection;26;1;57;0
WireConnection;31;0;29;0
WireConnection;31;1;39;0
WireConnection;25;0;20;0
WireConnection;25;1;24;0
WireConnection;32;0;31;0
WireConnection;32;1;2;0
WireConnection;32;2;26;1
WireConnection;23;0;14;0
WireConnection;23;1;24;0
WireConnection;21;0;32;0
WireConnection;21;1;25;0
WireConnection;22;0;21;0
WireConnection;22;1;23;0
WireConnection;0;1;22;0
ASEEND*/
//CHKSM=8169E9A1D98603400DE37EA0573D60720E2D6214