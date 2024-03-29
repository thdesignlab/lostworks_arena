/**
	SpriteStudio5 Player for Unity

	Copyright(C) Web Technology Corp. 
	All rights reserved.
*/
using UnityEngine;

using RandomGenerator = Library_SpriteStudio.Utility.Random.XorShift32;

[ExecuteInEditMode]
[System.Serializable]
public class Script_SpriteStudio_RootEffect : Library_SpriteStudio.Script.Root
{
	public enum Constants : int
	{
		LIMIT_PARTICLE = 1024,

		LIMIT_SUBEMITTER_DEPTH = 2,
		LIMIT_SUBEMITTER_COUNT = 10,
	}

	public enum PlayStyle
	{
		NO_CHANGE = -1,
		NORMAL = 0,
		PINGPONG = 1,
	}

	[System.Flags]
	private enum FlagBitStatus
	{
		VALID = 0x40000000,
		PLAYING = 0x20000000,
		PAUSING = 0x10000000,

		STYLE_PINGPONG = 0x08000000,	/* Reserved */
		STYLE_REVERSE = 0x04000000,	/* Reserved */

		PLAYING_START = 0x00800000,	/* Reserved */
		PLAYING_REVERSE = 0x00400000,	/* Reserved */
		PLAYING_REVERSEPREVIOUS = 0x00200000,	/* Reserved */
		PLAYING_TURN = 0x00100000,	/* Reserved */
		PLAYING_FIRSTUPDATE = 0x00080000,	/* Reserved */

//		DECODE_USERDATA = 0x00008000,	/* Reserved & Disuse */
//		DECODE_INSTANCE = 0x00004000,	/* Reserved & Disuse */
//		DECODE_EFFECT = 0x00002000,	/* Reserved & Disuse */

		REQUEST_DESTROY = 0x00000800,	/* Reserved */
		REQUEST_PLAYEND = 0x00000400,

		CLEAR = 0x00000000,
	}

	/* Base-Datas */
	public Script_SpriteStudio_DataCell DataCellMap;
	public Script_SpriteStudio_DataEffect DataEffect;

	/* Material Replacement Parameters  */
	/* MEMO: "IndexMaterialBlendDefault" & "IndexMaterialBlendOffset" is for Inspector & Importer. */
	public static readonly int[] IndexMaterialBlendDefault = new int[(int)Library_SpriteStudio.KindColorOperationEffect.TERMINATOR_KIND - 1]
	{
		(int)Library_SpriteStudio.KindColorOperationEffect.MIX - 1,
		(int)Library_SpriteStudio.KindColorOperationEffect.ADD - 1,
	};
	public static readonly int[] CountVariationShader = new int[(int)Library_SpriteStudio.KindColorOperationEffect.TERMINATOR_KIND - 1]
	{
		(int)Library_SpriteStudio.KindColorOperationEffect.MIX - (int)Library_SpriteStudio.KindColorOperationEffect.MIX,
		(int)Library_SpriteStudio.KindColorOperationEffect.ADD2 - (int)Library_SpriteStudio.KindColorOperationEffect.ADD,
	};
	public int[] IndexMaterialBlendOffset;

	/* Effect Datas */
	internal float RateTimeToFrame;
	internal float FrameNow;
	internal float FrameLength;

	/* Control Datas */
	public int CountLimitParticleInitial;
	internal Library_SpriteStudio.Control.PoolPartsEffect PoolParts = null;
	internal bool FlagUnderControl = false;

	/* Playing Datas: for Runtime (WorkArea) */
	private FlagBitStatus Status = FlagBitStatus.CLEAR;	
	public bool StatusIsPlaying
	{
		get
		{
			return(0 != (Status & FlagBitStatus.PLAYING));
		}
	}
	public bool StatusIsPausing
	{
		get
		{
			return(0 != (Status & FlagBitStatus.PAUSING));
		}
	}
	internal bool StatusIsPlayStylePingpong
	{
		get
		{
			return(0 != (Status & FlagBitStatus.STYLE_PINGPONG));
		}
	}
	internal bool StatusIsPlayStyleReverse
	{
		get
		{
			return(0 != (Status & FlagBitStatus.STYLE_REVERSE));
		}
	}
	internal bool StatusIsPlayingStart
	{
		get
		{
			return(0 != (Status & FlagBitStatus.PLAYING_START));
		}
	}
	internal bool StatusIsPlayingReverse
	{
		get
		{
			return(0 != (Status & FlagBitStatus.PLAYING_REVERSE));
		}
	}
	internal bool StatusIsPlayingReversePrevious
	{
		get
		{
			return(0 != (Status & FlagBitStatus.PLAYING_REVERSEPREVIOUS));
		}
	}
	internal bool StatusIsPlayingTurn
	{
		get
		{
			return(0 != (Status & FlagBitStatus.PLAYING_TURN));
		}
	}
//	internal bool StatusIsDecodeUserData
//	{
//		get
//		{
//			return(0 != (Status & FlagBitStatus.DECODE_USERDATA));
//		}
//	}
//	internal bool StatusIsDecodeInstance
//	{
//		get
//		{
//			return(0 != (Status & FlagBitStatus.DECODE_INSTANCE));
//		}
//	}
//	internal bool StatusIsDecodeEffect
//	{
//		get
//		{
//			return(0 != (Status & FlagBitStatus.DECODE_EFFECT));
//		}
//	}
	internal bool StatusIsRequestDestroy
	{
		get
		{
			return(0 != (Status & FlagBitStatus.REQUEST_DESTROY));
		}
	}
	internal bool StatusIsRequestPlayEnd
	{
		get
		{
			return(0 != (Status & FlagBitStatus.REQUEST_PLAYEND));
		}
	}

	/* CallBack-s */
	internal Library_SpriteStudio.FunctionCallBackPlayEndEffect FunctionPlayEnd = null;

	void Awake()
	{
		foreach(Material MaterialNow in TableMaterial)
		{
			MaterialNow.shader = Shader.Find(MaterialNow.shader.name);
		}
	}

	void Start()
	{
		/* Base Start */
		StartBase((int)Constants.LIMIT_PARTICLE);

		/* Initialize */
		if(null != DataEffect)
		{
			/* WorkArea BootUp */
			RateTimeToFrame = (float)DataEffect.CountFramePerSecond;	/* Provisional */
			TimePerFrame = 1.0f / RateTimeToFrame;
			PoolBootUpParts();

			/* Status Set */
			Status |= FlagBitStatus.VALID;

			/* Play Animation Initialize */
			if(false == FlagUnderControl)
			{
				AnimationPlay();
			}
		}
	}

//	void Update()
//	{
//	}

	void LateUpdate()
	{
		float TimeDelta = Time.deltaTime * RateSpeed;

		if((null == DataEffect) || (null == DataCellMap))
		{
			return;
		}

		/* DrawManager Get */
		if((null == InstanceManagerDraw) && (null == InstanceRootParent))
		{	/* MEMO: "Instance" and "Effect" cannot have Manager-Draw. */
			InstanceManagerDraw = Library_SpriteStudio.Utility.Parts.ManagerDrawGetParent(gameObject);
		}

		/* DrawParts-Cluster Create */
		LateUpdateBase();

		/* Animation Play-Check */
		if(0 == (Status & FlagBitStatus.PLAYING)) 
		{	/* Not-Playing */
			return;
		}

		/* Particle&Emitter Control WorkArea Create */
		if(null == PoolParts)
		{
			PoolBootUpParts();
		}

		/* Update Emitter & Particle */
		ChainClusterDrawParts.ChainCleanUp();   /* DrawParts-Cluster-Chain Clear */
		TimeElapsed += ((0 != (Status & FlagBitStatus.PAUSING)) || (0 != (Status & FlagBitStatus.PLAYING_START))) ? 0.0f : TimeDelta;
		FrameNow = TimeElapsed * RateTimeToFrame;
		FrameNow = Mathf.Clamp(FrameNow, 0.0f, FrameLength);
		PoolParts.Update(this);

		/* Set to DrawManager */
		/* MEMO: Test */
		if(((null != InstanceManagerDraw) && (null != DrawObject)) && (false == FlagHideForce))
		{
			/* Set To Draw-Manager */
			InstanceManagerDraw.DrawSet(this);
		}

		/* Status Update */
		Status &= ~FlagBitStatus.PLAYING_START;
	}
	internal void TimeElapsedSetForce(float TimeElapsedForce, bool FlagReverseParent)
	{   /* MEMO: In principle, This Function is for calling from "(Control.PartsEffect.)Update". */
		TimeElapsed = TimeElapsedForce;
	}

	private bool PoolBootUpParts()
	{
		int Count = CountLimitParticleInitial;
		if(0 >= Count)
		{
			Count = (int)Constants.LIMIT_PARTICLE;
		}

		PoolParts = new Library_SpriteStudio.Control.PoolPartsEffect();
		PoolParts.BootUpWorkArea(this, Count);
		PoolParts.BootUp(this);

		FrameLength = (float)(PoolParts.EffectDurationFull);

		return(true);
	}

	public static Library_SpriteStudio.Utility.Random.Generator InstanceCreateRandom()
	{
		return(new RandomGenerator());
	}

	private readonly static System.DateTime TimeUnixEpoch = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
	private static uint RandomKeyMakeID = 123456;
	public static uint KeyCreateRandom()
	{
		RandomKeyMakeID++;

		/* MEMO: time(0) at C++ */
		System.DateTime TimeNow = System.DateTime.Now;
		TimeNow.ToUniversalTime();
		System.TimeSpan SecNow = TimeNow - TimeUnixEpoch;
		
		return(RandomKeyMakeID + (uint)SecNow.TotalSeconds);
	}

	internal void SeedOffsetSet(uint Value)
	{
		if(null != PoolParts)
		{
			PoolParts.SeedOffsetSet(Value);
		}
	}

	public void TableCreateBlendOffset()
	{
		int CountBlendKind = (int)Library_SpriteStudio.KindColorOperationEffect.TERMINATOR_KIND - 1;
		if((null == IndexMaterialBlendOffset) || (CountBlendKind != IndexMaterialBlendOffset.Length))
		{
			IndexMaterialBlendOffset = new int[CountBlendKind];
			for(int i=0; i<CountBlendKind; i++)
			{
				IndexMaterialBlendOffset[i] = 0;
			}
		}
	}

	/* ******************************************************** */
	//! Start playing the animation
	/*!
	@param	FrameInitial
		Offset frame-number of starting Play in animation (0 origins). <br>
		At the time of the first play-loop, Animation is started "LabelStart + FrameOffsetStart + FrameInitial".
		-1 == use "FrameNoInitial" Value<br>
		default: -1
	@param	RateTime
		Coefficient of time-passage of animation.<br>
		Minus Value is given, Animation is played backwards.<br>
		0.0f is given, the now-setting is not changed) <br>
		default: 0.0f (Setting is not changed)
	@retval	Return-Value
		true == Success <br>
		false == Failure (Error)

	The playing of animation begins. <br>
	<br>
	The update speed of animation quickens when you give a value that is bigger than 1.0f to "RateTime".
	*/
	public bool AnimationPlay(	bool PlayLoop = false,
								float RateSpeedTimeProgress = 1.0f
							)
	{
		/* Check Fatal-Error */
		if((null == DataCellMap) || (null == DataEffect))
		{
			return(false);
		}

		/* Pool Refresh */
		if(null == PoolParts)
		{
			PoolBootUpParts();
		}

		RateSpeed = RateSpeedTimeProgress;
		RateTimeToFrame = (null != InstanceRootParent) ? (1.0f / InstanceRootParent.TimePerFrame) : (float)DataEffect.CountFramePerSecond;
		TimeElapsed = (0.0f > RateSpeed) ? (FrameLength * TimePerFrame) : 0.0f; 

		/* Status Set */
		Status |= FlagBitStatus.PLAYING;
		Status |= FlagBitStatus.PLAYING_START;

		return(true);
	}

	/* ******************************************************** */
	//! Stop playing the animation
	/*!
	@param	FlagWarpEnd
		Specifying Display-Frame<br>
		true == End-Frame<br>
		false == Status-quo<br>
		default: false
	@retval	Return-Value
		(None)

	The playing of animation stops.
	*/
	public void AnimationStop()
	{
		/* Pool Refresh */
		if(null != PoolParts)
		{
			PoolParts.ParticleReset();
		}

		/* Status Set */
		Status &= ~FlagBitStatus.PLAYING;

		return;
	}

	/* ******************************************************** */
	//! Set the pause-status of the animation
	/*!
	@param	FlagSwitch
		true == Set pause (Suspend)<br>
		false == Rerease pause (Resume)
	@retval	Return-Value
		true == Success <br>
		false == Failure (Error)

	The playing of animation suspends or resumes.<br>
	This function fails if the animation is not playing.
	*/
	public bool AnimationPause(bool FlagSwitch)
	{
		if(0 == (Status & FlagBitStatus.PLAYING))
		{
			return(false);
		}

		if(true == FlagSwitch)
		{
			if(0 == (Status & FlagBitStatus.PAUSING))
			{
				Status |= FlagBitStatus.PAUSING;
			}
		}
		else
		{
			if(0 != (Status & FlagBitStatus.PAUSING))
			{
				Status &= ~FlagBitStatus.PAUSING;
			}
		}

		return(false);
	}

	/* ******************************************************** */
	//! Check the animation is playing
	/*!
	@param
		(None)
	@retval	Return-Value
		true == Playing / Pause-true(suspended) <br>
		false == Stopping

	Use this function for checking the animation's play-status.
	*/
	public bool AnimationCheckPlay()
	{
		return(StatusIsPlaying);
	}

	/* ******************************************************** */
	//! Check if the animation is being paused (suspended)
	/*!
	@param
		(None)
	@retval	Return-Value
		true == Suspended <br>
		false == Not Suspended or Stopping

	Use this function for checking the animation's pause-status.
	*/
	public bool AnimationCheckPause()
	{
		return((true == StatusIsPlaying) ? StatusIsPausing : false);
	}

	/* ******************************************************** */
	//! Get Material
	/*!
	@param	IndexCellMap
		Serial-number of using Cell-Map
	@param	Operation
		Color-Blend Operation for the target
	@retval	Return-Value
		Material
	*/
	public Material MaterialGet(int IndexCellMap, Library_SpriteStudio.KindColorOperationEffect KindOperation)
	{
#if false
		return((	(0 <= IndexCellMap) && (DataCellMap.ListDataCellMap.Length > IndexCellMap)
					&& (Library_SpriteStudio.KindColorOperationEffect.NON < KindOperation) && (Library_SpriteStudio.KindColorOperationEffect.TERMINATOR > KindOperation)
				)
				? TableMaterial[(IndexCellMap * ((int)Library_SpriteStudio.KindColorOperationEffect.TERMINATOR - 1)) + ((int)KindOperation - 1)]
				: null
			);
#else
		if((0 > IndexCellMap) && (DataCellMap.ListDataCellMap.Length <= IndexCellMap))
		{
			return(null);
		}
		if((Library_SpriteStudio.KindColorOperationEffect.NON >= KindOperation) && (Library_SpriteStudio.KindColorOperationEffect.TERMINATOR_KIND <= KindOperation))
		{
			return(null);
		}
		int IndexBlendKind = (int)KindOperation - 1;
		int IndexMaterial = IndexMaterialBlendDefault[IndexBlendKind];
		if((null != IndexMaterialBlendOffset) && (0 < IndexMaterialBlendOffset.Length))
		{
			IndexMaterial += IndexMaterialBlendOffset[IndexBlendKind];
		}
		return(TableMaterial[(IndexCellMap * ((int)Library_SpriteStudio.KindColorOperationEffect.TERMINATOR - 1)) + IndexMaterial]);
#endif
	}

	/* ******************************************************** */
	//! Copy (Dupicate) Material-Table
	/*!
	@param
		(none)
	@retval	Return-Value
		Created Copy Material-Table<br>
		null == Failure

	Generate a duplicate of "TableMaterial".<br>
	Caution: The result is another instance the "TableMaterial".<br>
	*/
	public Material[] TableMaterialCopy()
	{
		return(Library_SpriteStudio.Utility.TableMaterial.Copy(TableMaterial));
	}

	/* ******************************************************** */
	//! Get count of Textures
	/*!
	@param
		(none)
	@retval	Return-Value
		Count of Texture<br>
		0 == Failure

	Get count of textures that can be set to "TableMaterial".
	*/
	public int TextureGetCount()
	{
		return(Library_SpriteStudio.Utility.TableMaterial.CountGetTextureEffect(TableMaterial));
	}

	/* ******************************************************** */
	//! Change 1-Texture in Material-Table
	/*!
	@param	Index
		Texture No.
	@param	DataTexture
		Texture
	@retval	Return-Value
		true == Success<br>
		false == Failure

	Change 1-Texture is set in "TableMaterial".
	Texture is set to "TableMaterial[Index * 2] to TableMaterial[Index * 2 + 1]".<br>
	<br>
	Appropriate range of "Material"-instance of the texture you want to change will be created in the new.<br>
	*/
	public bool TextureChange(int Index, Texture2D DataTexture)
	{
		int CountTextureBlock = (int)Library_SpriteStudio.KindColorOperationEffect.TERMINATOR - 1;

		if(null == TableMaterial)
		{
			return(false);
		}
		int CountTableMaterial = TableMaterial.Length;
		if(0 >= CountTableMaterial)
		{
			return(false);
		}

		int IndexMaterial = Index * CountTextureBlock;
		if((0 > IndexMaterial) || (CountTableMaterial <= IndexMaterial))
		{
			return(false);
		}
		for(int i=0; i<CountTextureBlock; i++)
		{
			TableMaterial[IndexMaterial + i] = new Material(Library_SpriteStudio.Shader_SpriteStudioEffect[i]);
			TableMaterial[IndexMaterial + i].mainTexture = DataTexture;
		}
		return(true);
	}
}
