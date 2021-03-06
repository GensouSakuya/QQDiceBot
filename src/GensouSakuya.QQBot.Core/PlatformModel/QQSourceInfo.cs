﻿namespace GensouSakuya.QQBot.Core.PlatformModel
{
	/// <summary>
	/// QQ信息
	/// </summary>
	public class QQSourceInfo
	{
		/// <summary>
		/// QQ号
		/// </summary>
		public long Id { get; set; }
		/// <summary>
		/// 性别
		/// </summary>
		public Sex Sex { get; set; }
		/// <summary>
		/// 昵称
		/// </summary>
		public string Nick { get; set; }
	}
}
