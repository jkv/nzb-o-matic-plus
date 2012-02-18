//------------------------------------------------------------------------------
//  <copyright from='2004' to='2005' company='Jerremy Koot and William Archbell'>
//    Copyright (c) Jerremy Koot and William Archbell. All Rights Reserved.
//  </copyright>
//-------------------------------------------------------------------------------
//
// File:    Interface.cs
//
// Purpose: This application connects to NNTP servers and downloads and combines
//          messages that are listed in the NZB files that are supplied by the user.

using System;

public interface IEngine
{
	void Start(string[] args);
	void Stop();

	bool Restart {get;set;}
	bool Update {get;set;}
}
