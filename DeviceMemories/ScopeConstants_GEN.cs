﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECore.DeviceMemories
{
#if DEBUG
	public
#else
	internal
#endif
	 enum REG
    {
		STROBE_UPDATE = 0,
		SPI_ADDRESS = 1,
		SPI_WRITE_VALUE = 2,
		DIVIDER_MULTIPLIER = 3,
		CHA_YOFFSET_VOLTAGE = 4,
		CHB_YOFFSET_VOLTAGE = 5,
		TRIGGER_PWM = 6,
		TRIGGER_LEVEL = 7,
		TRIGGER_THRESHOLD = 8,
		TRIGGER_MODE = 9,
		TRIGGER_WIDTH = 10,
		INPUT_DECIMATION = 11,
		ACQUISITION_DEPTH = 12,
		TRIGGERHOLDOFF_B0 = 13,
		TRIGGERHOLDOFF_B1 = 14,
		TRIGGERHOLDOFF_B2 = 15,
		TRIGGERHOLDOFF_B3 = 16,
		VIEW_DECIMATION = 17,
		VIEW_OFFSET_B0 = 18,
		VIEW_OFFSET_B1 = 19,
		VIEW_OFFSET_B2 = 20,
		VIEW_ACQUISITIONS = 21,
		VIEW_BURSTS = 22,
		VIEW_EXCESS_B0 = 23,
		VIEW_EXCESS_B1 = 24,
		DIGITAL_TRIGGER_RISING = 25,
		DIGITAL_TRIGGER_FALLING = 26,
		DIGITAL_TRIGGER_HIGH = 27,
		DIGITAL_TRIGGER_LOW = 28,
		DIGITAL_OUT = 29,
		AWG_DEBUG = 30,
		AWG_DECIMATION = 31,
		AWG_SAMPLES_B0 = 32,
		AWG_SAMPLES_B1 = 33,
    }

#if DEBUG
	public
#else
	internal
#endif
	 enum STR
    {
		GLOBAL_RESET = 0,
		INIT_SPI_TRANSFER = 1,
		AWG_ENABLE = 2,
		LA_ENABLE = 3,
		SCOPE_ENABLE = 4,
		SCOPE_UPDATE = 5,
		FORCE_TRIGGER = 6,
		VIEW_UPDATE = 7,
		ACQ_START = 9,
		ACQ_STOP = 10,
		CHA_DCCOUPLING = 13,
		CHB_DCCOUPLING = 14,
		ENABLE_ADC = 15,
		OVERFLOW_DETECT = 16,
		ENABLE_NEG = 17,
		ENABLE_RAM = 19,
		DOUT_3V_5V = 22,
		EN_OPAMP_B = 23,
		AWG_DEBUG = 24,
		DIGI_DEBUG = 25,
		ROLL = 26,
		LA_CHANNEL = 27,
    }

#if DEBUG
	public
#else
	internal
#endif
	 enum ROM
    {
		FW_MSB = 0,
		FW_LSB = 1,
		FW_GIT0 = 2,
		FW_GIT1 = 3,
		FW_GIT2 = 4,
		FW_GIT3 = 5,
		SPI_RECEIVED_VALUE = 6,
		STROBES = 7,
    }

}
