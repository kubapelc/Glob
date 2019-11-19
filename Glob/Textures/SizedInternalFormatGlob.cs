﻿using System;

namespace Glob
{
	/// <summary>
	/// Enum listing all possible sized internal formats
	/// </summary>
	public enum SizedInternalFormatGlob
	{
		R8 = 0x8229,
		R16 = 0x822A,
		RG8 = 0x822B,
		RG16 = 0x822C,
		R16F = 0x822D,
		R32F = 0x822E,
		RG16F = 0x822F,
		RG32F = 0x8230,
		R8I = 0x8231,
		R8UI = 0x8232,
		R16I = 0x8233,
		R16UI = 0x8234,
		R32I = 0x8235,
		R32UI = 0x8236,
		RG8I = 0x8237,
		RG8UI = 0x8238,
		RG16I = 0x8239,
		RG16UI = 0x823A,
		RG32I = 0x823B,
		RG32UI = 0x823C,

		R3_G3_B2 = 0x2A10,
		RGB4 = 0x804F,
		RGB5 = 0x8050,
		RGB8 = 0x8051,
		RGB10 = 0x8052,
		RGB12 = 0x8053,
		RGB16 = 0x8054,
		RGBA2 = 0x8055,
		RGBA4 = 0x8056,
		RGB5_A1 = 0x8057,
		RGBA8 = 0x8058,
		RGB10_A2 = 0x8059,
		RGBA12 = 0x805A,
		RGBA16 = 0x805B,

		R8_SNORM = 0x8F94,
		RG8_SNORM = 0x8F95,
		RGB8_SNORM = 0x8F96,
		RGBA8_SNORM = 0x8F97,
		R16_SNORM = 0x8F98,
		RG16_SNORM = 0x8F99,
		RGB16_SNORM = 0x8F9A,
		RGBA16_SNORM = 0x8F9B,

		RGB10_A2UI = 0x906F,

		SRGB8 = 0x8C41,
		SRGB8_ALPHA8 = 0x8C43,

		RGBA32F = 0x8814,
		RGB32F = 0x8815,
		RGBA16F = 0x881A,
		RGB16F = 0x881B,

		R11F_G11F_B10F = 0x8C3A,
		RGB9_E5 = 0x8C3D,

		RGBA32UI = 0x8D70,
		RGB32UI = 0x8D71,
		RGBA16UI = 0x8D76,
		RGB16UI = 0x8D77,
		RGBA8UI = 0x8D7C,
		RGB8UI = 0x8D7D,
		RGBA32I = 0x8D82,
		RGB32I = 0x8D83,
		RGBA16I = 0x8D88,
		RGB16I = 0x8D89,
		RGBA8I = 0x8D8E,
		RGB8I = 0x8D8F,

		DEPTH_COMPONENT16 = 0x81A5,
		DEPTH_COMPONENT24 = 0x81A6,
		DEPTH_COMPONENT32 = 0x81A7,

		DEPTH_COMPONENT32F = 0x8CAC,
		DEPTH32F_STENCIL8 = 0x8CAD,
		DEPTH24_STENCIL8 = 0x88F0,

		STENCIL_INDEX8 = 0x8D48,

		COMPRESSED_RED_RGTC1 = 0x8DBB,
		COMPRESSED_SIGNED_RED_RGTC1 = 0x8DBC,
		COMPRESSED_RG_RGTC2 = 0x8DBD,
		COMPRESSED_SIGNED_RG_RGTC2 = 0x8DBE,
		COMPRESSED_RED = 0x8225,
		COMPRESSED_RG = 0x8226,
		COMPRESSED_RGB = 0x84ED,
		COMPRESSED_RGBA = 0x84EE,
		COMPRESSED_SRGB = 0x8C48,
		COMPRESSED_SRGB_ALPHA = 0x8C49,
		COMPRESSED_RGBA_BPTC_UNORM = 0x8E8C,
		COMPRESSED_SRGB_ALPHA_BPTC_UNORM = 0x8E8D,
		COMPRESSED_RGB_BPTC_SIGNED_FLOAT = 0x8E8E,
		COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT = 0x8E8F,

		COMPRESSED_RGB_S3TC_DXT1_EXT = 0x83F0,
		COMPRESSED_RGBA_S3TC_DXT1_EXT = 0x83F1,
		COMPRESSED_RGBA_S3TC_DXT3_EXT = 0x83F2,
		COMPRESSED_RGBA_S3TC_DXT5_EXT = 0x83F3,

		COMPRESSED_SRGB_S3TC_DXT1_EXT = 0x8C4C,
		COMPRESSED_SRGB_ALPHA_S3TC_DXT1_EXT = 0x8C4D,
		COMPRESSED_SRGB_ALPHA_S3TC_DXT3_EXT = 0x8C4E,
		COMPRESSED_SRGB_ALPHA_S3TC_DXT5_EXT = 0x8C4F,
	}
}
