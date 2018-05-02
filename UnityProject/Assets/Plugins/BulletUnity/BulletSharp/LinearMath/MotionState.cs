﻿using System;
using System.Runtime.InteropServices;
using System.Security;
using BulletSharp.Math;
using AOT;

namespace BulletSharp
{
	public abstract class MotionState : IDisposable
	{
		internal IntPtr _native;

		[UnmanagedFunctionPointer(Native.Conv), SuppressUnmanagedCodeSecurity]
		private delegate void GetWorldTransformUnmanagedDelegate(IntPtr thisPtr, out Matrix worldTrans);
		[UnmanagedFunctionPointer(Native.Conv), SuppressUnmanagedCodeSecurity]
		private delegate void SetWorldTransformUnmanagedDelegate(IntPtr thisPtr, ref Matrix worldTrans);

		private GetWorldTransformUnmanagedDelegate _getWorldTransform;
		private SetWorldTransformUnmanagedDelegate _setWorldTransform;

		internal MotionState(IntPtr native)
		{
			_native = native;
		}

		protected MotionState()
		{
			_getWorldTransform = new GetWorldTransformUnmanagedDelegate(GetWorldTransformUnmanaged);
			_setWorldTransform = new SetWorldTransformUnmanagedDelegate(SetWorldTransformUnmanaged);
            GCHandle handle = GCHandle.Alloc(this, GCHandleType.Normal);

            _native = UnsafeNativeMethods.btMotionStateWrapper_new(
				Marshal.GetFunctionPointerForDelegate(_getWorldTransform),
				Marshal.GetFunctionPointerForDelegate(_setWorldTransform),
                GCHandle.ToIntPtr(handle)
                );
		}

        [MonoPInvokeCallback(typeof(GetWorldTransformUnmanagedDelegate))]
        static void GetWorldTransformUnmanaged(IntPtr msPtr, out Matrix worldTrans)
		{
            MotionState ms = GCHandle.FromIntPtr(msPtr).Target as MotionState;
            ms.GetWorldTransform(out worldTrans);
		}

        [MonoPInvokeCallback(typeof(SetWorldTransformUnmanagedDelegate))]
        static void SetWorldTransformUnmanaged(IntPtr msPtr, ref Matrix worldTrans)
		{
            MotionState ms = GCHandle.FromIntPtr(msPtr).Target as MotionState;
            ms.SetWorldTransform(ref worldTrans);
		}

		public abstract void GetWorldTransform(out Matrix worldTrans);
		public abstract void SetWorldTransform(ref Matrix worldTrans);

		public Matrix WorldTransform
		{
			get
			{
				Matrix transform;
				GetWorldTransform(out transform);
				return transform;
			}
			set {  SetWorldTransform(ref value);}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_native != IntPtr.Zero)
			{
				UnsafeNativeMethods.btMotionState_delete(_native);
				_native = IntPtr.Zero;
			}
		}

		~MotionState()
		{
			Dispose(false);
		}
	}
}
