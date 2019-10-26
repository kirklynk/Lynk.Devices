using Lynk.Devices.Shared;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
//using System.Runtime.CompilerServices;

namespace Lynk.Devices.BNO055
{
	public class Bno055
	{
		private readonly II2cDevice _i2cDevice;

		public Bno055(II2cDevice device, bool hasExtCrystal = false)
		{
			_i2cDevice = device;
			Initialize(hasExtCrystal);
		}

		private void Initialize(bool hasExtCrystal)
		{
			Thread.Sleep(1000);
			SetExternalCrystal(hasExtCrystal);
			Thread.Sleep(1000);

			UpdateUnitSelection();
			Thread.Sleep(1000);

			SetOperationMode(OperatingMode.NDOF);
			Thread.Sleep(1000);
		}

		public enum AccRangeOptions { Range2G = 0, Range4G, Range8G, Range16G }
		public enum AccBandwidthOptions { HZ_7_81 = 0, HZ_15_63, HZ_31_25, HZ_62_50, HZ_125, HZ_250, HZ_500, HZ_1000 }
		public enum AccOperationModeOptions { Normal, Suspend, LowPowerOne, StandBy, LowPower2, DeepSuspend }

		public enum GyroRangeOptions { DPS2000 = 0, DPS1000, DPS500, DPS250, DPS125 }
		public enum GyroBandwidthOptions { Hz_523, Hz_230, Hz_116, HZ_47, HZ_23, HZ_12, HZ_64, HZ_32 }
		public enum GyroOperationModeOptions { Normal, FastPowerUp, DeepSuspend, Suspend, AdvancedPowerSave }

		public enum MagOutputRateOptions { TwoHz, SixHz, OneZeroHz, OneFiveHz, TwoZeroHz, TwoFiveHz, ThreeFiveHz }
		public enum MagOperationModeOptions { LowerPower, Regular, EnhancedRegular, HighAccuracy }
		public enum MagPowerModeOptions { Normal, Sleep, Suspend, ForceMode }

		public enum OperatingMode : byte
		{
			CONFIGMODE = 0x00,
			ACCONLY = 0x01,
			MAGONLY = 0x02,
			GYROONLY = 0x03,
			ACCMAG = 0x04,
			ACCGYRO = 0x05,
			MAGGRYO = 0x06,
			AMG = 0x07,
			IMU = 0x08,
			COMPASS = 0x09,
			M4G = 0x0A,
			NDOF_FMC_OFF = 0x0B,
			NDOF = 0x0C
		}

		public class REGISTERS
		{

			/***************************************************/
			/**\name	REGISTER ADDRESS DEFINITION  */
			/***************************************************/
			/* Page id register definition*/
			public const byte PAGE_ID = 0x07;

			/* PAGE0 REGISTER DEFINITION START*/
			public const byte CHIP_ID = 0x00;
			public const byte ACCEL_REV_ID = 0x01;
			public const byte MAG_REV_ID = 0x02;
			public const byte GYRO_REV_ID = 0x03;
			public const byte SW_REV_ID_LSB = 0x04;
			public const byte SW_REV_ID_MSB = 0x05;
			public const byte BL_REV_ID = 0x06;

			/* Accel data register*/
			public const byte ACCEL_DATA_X_LSB = 0x08;
			public const byte ACCEL_DATA_X_MSB = 0x09;
			public const byte ACCEL_DATA_Y_LSB = 0x0A;
			public const byte ACCEL_DATA_Y_MSB = 0x0B;
			public const byte ACCEL_DATA_Z_LSB = 0x0C;
			public const byte ACCEL_DATA_Z_MSB = 0x0D;

			/*Mag data register*/
			public const byte MAG_DATA_X_LSB = 0x0E;
			public const byte MAG_DATA_X_MSB = 0x0F;
			public const byte MAG_DATA_Y_LSB = 0x10;
			public const byte MAG_DATA_Y_MSB = 0x11;
			public const byte MAG_DATA_Z_LSB = 0x12;
			public const byte MAG_DATA_Z_MSB = 0x13;

			/*Gyro data registers*/
			public const byte GYRO_DATA_X_LSB = 0x14;
			public const byte GYRO_DATA_X_MSB = 0x15;
			public const byte GYRO_DATA_Y_LSB = 0x16;
			public const byte GYRO_DATA_Y_MSB = 0x17;
			public const byte GYRO_DATA_Z_LSB = 0x18;
			public const byte GYRO_DATA_Z_MSB = 0x19;

			/*Euler data registers*/
			public const byte EULER_H_LSB = 0x1A;
			public const byte EULER_H_MSB = 0x1B;
			public const byte EULER_R_LSB = 0x1C;
			public const byte EULER_R_MSB = 0x1D;
			public const byte EULER_P_LSB = 0x1E;
			public const byte EULER_P_MSB = 0x1F;

			/*Quaternion data registers*/
			public const byte QUATERNION_DATA_W_LSB = 0x20;
			public const byte QUATERNION_DATA_W_MSB = 0x21;
			public const byte QUATERNION_DATA_X_LSB = 0x22;
			public const byte QUATERNION_DATA_X_MSB = 0x23;
			public const byte QUATERNION_DATA_Y_LSB = 0x24;
			public const byte QUATERNION_DATA_Y_MSB = 0x25;
			public const byte QUATERNION_DATA_Z_LSB = 0x26;
			public const byte QUATERNION_DATA_Z_MSB = 0x27;

			/* Linear acceleration data registers*/
			public const byte LINEAR_ACCEL_DATA_X_LSB = 0x28;
			public const byte LINEAR_ACCEL_DATA_X_MSB = 0x29;
			public const byte LINEAR_ACCEL_DATA_Y_LSB = 0x2A;
			public const byte LINEAR_ACCEL_DATA_Y_MSB = 0x2B;
			public const byte LINEAR_ACCEL_DATA_Z_LSB = 0x2C;
			public const byte LINEAR_ACCEL_DATA_Z_MSB = 0x2D;

			/*Gravity data registers*/
			public const byte GRAVITY_DATA_X_LSB = 0x2E;
			public const byte GRAVITY_DATA_X_MSB = 0x2F;
			public const byte GRAVITY_DATA_Y_LSB = 0x30;
			public const byte GRAVITY_DATA_Y_MSB = 0x31;
			public const byte GRAVITY_DATA_Z_LSB = 0x32;
			public const byte GRAVITY_DATA_Z_MSB = 0x33;

			/* Temperature data register*/
			public const byte TEMP = 0x34;

			/* Status registers*/
			public const byte CALIB_STAT = 0x35;
			public const byte SELFTEST_RESULT = 0x36;
			public const byte INTR_STAT = 0x37;
			public const byte SYS_CLK_STAT = 0x38;
			public const byte SYS_STAT = 0x39;
			public const byte SYS_ERR = 0x3A;

			/* Unit selection register*/
			public const byte UNIT_SEL = 0x3B;
			public const byte DATA_SELECT = 0x3C;

			/* Mode registers*/
			public const byte OPR_MODE = 0x3D;
			public const byte PWR_MODE = 0x3E;

			public const byte SYS_TRIGGER = 0x3F;
			public const byte TEMP_SOURCE = 0x40;
			/* Axis remap registers*/
			public const byte AXIS_MAP_CONFIG = 0x41;
			public const byte AXIS_MAP_SIGN = 0x42;

			/* SIC registers*/
			public const byte SIC_MATRIX_0_LSB = 0x43;
			public const byte SIC_MATRIX_0_MSB = 0x44;
			public const byte SIC_MATRIX_1_LSB = 0x45;
			public const byte SIC_MATRIX_1_MSB = 0x46;
			public const byte SIC_MATRIX_2_LSB = 0x47;
			public const byte SIC_MATRIX_2_MSB = 0x48;
			public const byte SIC_MATRIX_3_LSB = 0x49;
			public const byte SIC_MATRIX_3_MSB = 0x4A;
			public const byte SIC_MATRIX_4_LSB = 0x4B;
			public const byte SIC_MATRIX_4_MSB = 0x4C;
			public const byte SIC_MATRIX_5_LSB = 0x4D;
			public const byte SIC_MATRIX_5_MSB = 0x4E;
			public const byte SIC_MATRIX_6_LSB = 0x4F;
			public const byte SIC_MATRIX_6_MSB = 0x50;
			public const byte SIC_MATRIX_7_LSB = 0x51;
			public const byte SIC_MATRIX_7_MSB = 0x52;
			public const byte SIC_MATRIX_8_LSB = 0x53;
			public const byte SIC_MATRIX_8_MSB = 0x54;

			/* Accelerometer Offset registers*/
			public const byte ACCEL_OFFSET_X_LSB = 0x55;
			public const byte ACCEL_OFFSET_X_MSB = 0x56;
			public const byte ACCEL_OFFSET_Y_LSB = 0x57;
			public const byte ACCEL_OFFSET_Y_MSB = 0x58;
			public const byte ACCEL_OFFSET_Z_LSB = 0x59;
			public const byte ACCEL_OFFSET_Z_MSB = 0x5A;

			/* Magnetometer Offset registers*/
			public const byte MAG_OFFSET_X_LSB = 0x5B;
			public const byte MAG_OFFSET_X_MSB = 0x5C;
			public const byte MAG_OFFSET_Y_LSB = 0x5D;
			public const byte MAG_OFFSET_Y_MSB = 0x5E;
			public const byte MAG_OFFSET_Z_LSB = 0x5F;
			public const byte MAG_OFFSET_Z_MSB = 0x60;

			/* Gyroscope Offset registers*/
			public const byte GYRO_OFFSET_X_LSB = 0x61;
			public const byte GYRO_OFFSET_X_MSB = 0x62;
			public const byte GYRO_OFFSET_Y_LSB = 0x63;
			public const byte GYRO_OFFSET_Y_MSB = 0x64;
			public const byte GYRO_OFFSET_Z_LSB = 0x65;
			public const byte GYRO_OFFSET_Z_MSB = 0x66;

			/* Radius registers*/
			public const byte ACCEL_RADIUS_LSB = 0x67;
			public const byte ACCEL_RADIUS_MSB = 0x68;
			public const byte MAG_RADIUS_LSB = 0x69;
			public const byte MAG_RADIUS_MSB = 0x6A;
			/* PAGE0 REGISTERS DEFINITION END*/

			/* PAGE1 REGISTERS DEFINITION START*/
			/* Configuration registers*/
			public const byte ACCEL_CONFIG = 0x08;
			public const byte MAG_CONFIG = 0x09;
			public const byte GYRO_CONFIG_0 = 0x0A;
			public const byte GYRO_CONFIG_1 = 0x0B;
			public const byte ACCEL_SLEEP_CONFIG = 0x0C;
			public const byte GYRO_SLEEP_CONFIG = 0x0D;
			public const byte MAG_SLEEP_CONFIG = 0x0E;

			/* Interrupt registers*/
			public const byte INT_MASK = 0x0F;
			public const byte INT = 0x10;
			public const byte ACCEL_ANY_MOTION_THRES = 0x11;
			public const byte ACCEL_INTR_SETTINGS = 0x12;
			public const byte ACCEL_HIGH_G_DURN = 0x13;
			public const byte ACCEL_HIGH_G_THRES = 0x14;
			public const byte ACCEL_NO_MOTION_THRES = 0x15;
			public const byte ACCEL_NO_MOTION_SET = 0x16;
			public const byte GYRO_INTR_SETING = 0x17;
			public const byte GYRO_HIGHRATE_X_SET = 0x18;
			public const byte GYRO_DURN_X = 0x19;
			public const byte GYRO_HIGHRATE_Y_SET = 0x1A;
			public const byte GYRO_DURN_Y = 0x1B;
			public const byte GYRO_HIGHRATE_Z_SET = 0x1C;
			public const byte GYRO_DURN_Z = 0x1D;
			public const byte GYRO_ANY_MOTION_THRES = 0x1E;
			public const byte GYRO_ANY_MOTION_SET = 0x1F;


			/* PAGE1 REGISTERS DEFINITION END*/
		}

		public bool IsAccelerationGravityVector { get; set; }
		public bool IsAngularRateInRps { get; set; }
		public bool IsEulerAnglesInRadians { get; set; }
		public bool IsTemperatureInF { get; set; }
		public bool IsAndroidOutputFormat { get; set; }

		public OperatingMode Mode { get; private set; } = OperatingMode.CONFIGMODE; //Default after power on 

		public void UpdateUnitSelection()
		{
			_i2cDevice.Write(REGISTERS.UNIT_SEL);
			byte newVal = _i2cDevice.Read();

			newVal &= IsAndroidOutputFormat ? (byte)0b_1111_1111 : (byte)0b_0111_1111;
			newVal &= IsTemperatureInF ? (byte)0b_1111_1111 : (byte)0b_1110_1111;
			newVal &= IsEulerAnglesInRadians ? (byte)0b_1111_1111 : (byte)0b_1111_0111;
			newVal &= IsAngularRateInRps ? (byte)0b_1111_1111 : (byte)0b_1111_1101;
			newVal &= IsAccelerationGravityVector ? (byte)0b_1111_1111 : (byte)0b_1111_1110;

			_i2cDevice.Write(new ReadOnlySpan<byte>(new byte[] { REGISTERS.UNIT_SEL, newVal }));
		}

		public void SetOperationMode(OperatingMode mode)
		{
			if (Mode == mode)
				return;

			_i2cDevice.Write(REGISTERS.OPR_MODE);
			var current = _i2cDevice.Read();
			current &= 0b_1111_0000; //Clear last 4 bits;
			current |= (byte)mode;
			_i2cDevice.Write(new ReadOnlySpan<byte>(new byte[] { REGISTERS.OPR_MODE, current }));
		}

		public void SetAccelerometer(AccOperationModeOptions accOperation, AccBandwidthOptions accBandwidth, AccRangeOptions accRange)
		{
			CheckOperatingMode();
			byte newValue = (byte)((byte)accOperation << 6);
			newValue &= (byte)((byte)accBandwidth << 3);
			newValue &= (byte)accRange;
			_i2cDevice.Write(new ReadOnlySpan<byte>(new byte[] { REGISTERS.MAG_CONFIG, newValue }));
		}

		public void SetGyroscope(GyroRangeOptions range, GyroBandwidthOptions bandwidth, GyroOperationModeOptions operationMode)
		{
			CheckOperatingMode();
			_i2cDevice.Write(REGISTERS.GYRO_CONFIG_0);
			var gyr_conf_0 = _i2cDevice.Read();
			gyr_conf_0 &= 0b_1100_0000;
			gyr_conf_0 &= (byte)((byte)bandwidth << 3);
			gyr_conf_0 &= (byte)range;
			_i2cDevice.Write(new ReadOnlySpan<byte>(new byte[]{ REGISTERS.GYRO_CONFIG_0, gyr_conf_0 }));

			_i2cDevice.Write(REGISTERS.GYRO_CONFIG_1);
			var gyr_conf_1 = _i2cDevice.Read();
			gyr_conf_1 &= 0b_1111_1000;
			gyr_conf_1 &= (byte)operationMode;
			_i2cDevice.Write(new ReadOnlySpan<byte>(new byte[] { REGISTERS.GYRO_CONFIG_1, gyr_conf_1 }));
		}

		public void SetMagnetometer(MagPowerModeOptions powerMode, MagOperationModeOptions operationMode, MagOutputRateOptions outputRateOption)
		{
			CheckOperatingMode();
			byte newValue = (byte)((byte)powerMode << 6);
			newValue &= (byte)((byte)operationMode << 3);
			newValue &= (byte)outputRateOption;
			_i2cDevice.Write(new ReadOnlySpan<byte>(new byte[] { REGISTERS.MAG_CONFIG, newValue }));
		}

		/// <summary>
		/// The compensated fused orientation data in Euler angles Heading(Z)/Roll(X)/Pitch(Y)  
		/// </summary>
		/// <returns></returns>
		public Vector3 GetOrientation()
		{
			Span<byte> val = stackalloc byte[6];
			_i2cDevice.Write(REGISTERS.EULER_H_LSB);
			_i2cDevice.Read(val);
			var heading = BinaryPrimitives.ReadInt16LittleEndian(val);
			var roll = BinaryPrimitives.ReadInt16LittleEndian(val.Slice(2));
			var pitch = BinaryPrimitives.ReadInt16LittleEndian(val.Slice(4));
			var retVect = new Vector3(roll, pitch, heading);

			if (IsAngularRateInRps)
				return retVect / 900;
			else
				return retVect / 16;
		}

		public void SetExternalCrystal(bool hasExtCrystal)
		{
			_i2cDevice.Write(REGISTERS.SYS_TRIGGER);
			byte trigger = _i2cDevice.Read();
			trigger &= hasExtCrystal ? (byte)0b_1111_1111 : (byte)0b_0111_1111;
			_i2cDevice.Write(new ReadOnlySpan<byte>(new byte[] { REGISTERS.SYS_TRIGGER, trigger }));
		}

		private void CheckOperatingMode([CallerMemberName]string name = "")
		{
			if (Mode == OperatingMode.IMU || Mode == OperatingMode.COMPASS || Mode == OperatingMode.M4G || Mode == OperatingMode.NDOF_FMC_OFF || Mode == OperatingMode.NDOF)
			{
				throw new NotSupportedException($"Cannot perform that action while in fusion mode!");
			}
		}
	}


}
