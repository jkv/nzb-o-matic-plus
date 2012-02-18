//------------------------------------------------------------------------------
//  <copyright from='2004' to='2005' company='Jerremy Koot and William Archbell'>
//    Copyright (c) Jerremy Koot and William Archbell. All Rights Reserved.
//
//    Please look in the accompanying license.htm file for the license that 
//    applies to this source code. (a copy can also be found at: 
//    http://nzb.wordtgek.nl/license.htm)
//  </copyright>
//-------------------------------------------------------------------------------
//
// File:    Decoder.cs
//
// Purpose: This application connects to NNTP servers and downloads and combines
//          messages that are listed in the NZB files that are supplied by the user.

using System;
using System.Collections;
using System.Management;

namespace NZB_O_Matic
{
	/// <summary>
	/// Summary description for Decoder.
	/// </summary>
	public class Decoder
	{
		public Decoder()
		{
		}

		public enum DiskProperties
		{
			Access,
			Availability,
			BlockSize,
			Caption,
			Compressed,
			ConfigManagerErrorCode,
			ConfigManagerUserConfig,
			CreationClassName,
			Description,
			DeviceID,
			DriveType,
			ErrorCleared,
			ErrorDescription,
			ErrorMethodology,
			FileSystem,
			FreeSpace,
			InstallDate,
			LastErrorCode,
			MaximumComponentLength,
			MediaType,
			Name,
			NumberOfBlocks,
			PNPDeviceID,
			PowerManagementCapabilities,
			PowerManagementSupported,
			ProviderName,
			Purpose,
			Size,
			Status,
			StatusInfo,
			SupportsFileBasedCompression,
			SystemCreationClassName,
			SystemName,
			VolumeName,
			VolumeSerialNumber
		}


		public static ArrayQueue DecodeQueue;

		public static bool TerminateDecoder = false;
		public static System.Threading.Thread DecoderThread;

		public static void Decode()
		{
			Article article;
			while(!TerminateDecoder)
			{
				try
				{
					article = (Article)DecodeQueue.Dequeue();
				}
				catch
				{
					System.Threading.Thread.Sleep(250);
					article = null;
				}

				if( article != null)
				{
					if(article.Status == ArticleStatus.Deleted)
					{
						foreach(Segment segment in article.Segments)
						{
							try
							{
/*								if(System.IO.File.Exists(System.IO.Path.GetFullPath("Cache\\" + segment.ArticleID)))
									System.IO.File.Delete(System.IO.Path.GetFullPath("Cache\\" + segment.ArticleID));
*/
								if(System.IO.File.Exists(System.IO.Path.GetFullPath(Global.m_CacheDirectory + segment.ArticleID)))
									System.IO.File.Delete(System.IO.Path.GetFullPath(Global.m_CacheDirectory + segment.ArticleID));
							}
							catch(Exception q)
							{
								Console.WriteLine(q);
							}
						}
					}
					else
					{
						article.Status = ArticleStatus.Decoding;
					
						try
						{
							
							switch( DecodeArticle( article))
							{
								case DecodeStatus.Decoded:
									article.Status = ArticleStatus.Decoded;
									frmMain.LogWriteInfo("Article decoded succesfully: " + article.Filename);
									break;

								case DecodeStatus.FailedCRC:
									article.Status = ArticleStatus.Error;
									frmMain.LogWriteInfo("Article failed CRC check: " + article.Filename);
									break;

								case DecodeStatus.FailedNothingToDecode:
									article.Status = ArticleStatus.InternalError;
									frmMain.LogWriteInfo("Encountered an internal error on article: " + article.Filename);
									break;
							}
						}
						catch( Exception e)
						{
							frmMain.LogWriteError("Encountered an internal error on article: " + article.Filename + "[" + e.Message + "]");
							article.Status = ArticleStatus.InternalError;
						}
					}
				}
			}
		}

		public static int uudecode_dec( char c)
		{
			int result = ((c - 0x20) & 0x3F); 
			return result;
		}

		public static bool uudecode_is_dec( char c)
		{
			return (((c - 0x20) >= 0) &&  ((c - 0x20) <= (0x3F + 1)));
		}

		public static bool uudecode_checkline( char[] buffer)
		{
			int n = 0;

			n = buffer[0] - 0x20;

			if( n <= 0)
				return false;

			if( n > 45) // max size
				return false;

			if( ((n + 1)/3)*4 > (buffer.Length - 1))
				return false;

			foreach( char c in buffer)
				if( !uudecode_is_dec( c))
					return false;

			return true;
		}

		private static string GetDirectory(Article article)
		{
			if(Global.m_Options.SavePath == "")
				Global.m_Options.SavePath = System.IO.Path.GetFullPath(Global.m_DownloadDirectory);
			string path = Global.m_Options.SavePath;
			
			/* while(path.StartsWith("\\")) // Why wouldnt you want \\ paths ?
				path = path.Remove(0, 1);*/

			while(path.EndsWith("\\"))
				path = path.Substring( 0, path.Length - 1);

			path += "\\" + ReplaceVariables(Global.m_Options.SaveFolder, article);
			path = path.Replace( "?", "");
			path = path.Replace( "*", "");
			path = path.Replace( "|", "");
			path = path.Replace( "<", "");
			path = path.Replace( ">", "");
			return path;
		}

		private static string ReplaceVariables(string str, Article art)
		{
			char[] vars = {	  'x', //file extension
							  's', //article subject
							  'n', //file name
							  'g', //newsgroup
							  'p', //file poster
							  'S', //file size
							  'd', //post date
							  'D', //date now
							  't', //article status ----- NEED TO GET THIS WORKING, DETECT INCOMPLETE FILES ETC.
							  'i', //name of file imported from
							  'y', //same as z but replace _ with ' '
							  'z'  //if imported from a newzbin file this will strip the msgid_id_ off
						  };
			System.Text.RegularExpressions.RegexOptions options = 
				System.Text.RegularExpressions.RegexOptions.None;
			System.Text.RegularExpressions.Regex regex = 
				new System.Text.RegularExpressions.Regex(@"msgid_+\d*_", options);
			System.Text.RegularExpressions.Regex regex2 = 
				new System.Text.RegularExpressions.Regex(@"msgidlist_uid+\d*_", options);

			foreach(char c in vars)
			{
				int length = str.Length;
				for(int index = 0; index < length; index++)
				{
					if(length != str.Length)
					{
						index = 0;
						length = str.Length;
					}

					if(str[index] == c)
					{
						if( ( ( (index > 2) && str[index-1] == '%') && str[index-2] != '\\') || ( (index == 1) && str[index-1] == '%') )
						{
							string replace = "";
							switch(c)
							{
								case 'x':
									string temp = System.IO.Path.GetExtension(art.Filename).ToLower();
									while(temp.StartsWith("."))
										temp = temp.Remove(0, 1);
									replace = temp;
									break;
								case 's':
									replace = art.Subject.ToLower();

									replace = replace.Replace( ":", "");
									replace = replace.Replace( "\\", "");
									replace = replace.Replace( "/", "");
									break;
								case 'n':
									replace = art.Filename;
									break;
								case 'g':
									replace = art.Groups[0].ToLower();
									break;
								case 'p':
									replace = art.Poster.ToLower();

									replace = replace.Replace( ":", "");
									replace = replace.Replace( "\\", "");
									replace = replace.Replace( "/", "");
									break;
								case 'S':
									replace = art.Size.ToString();
									break;
								case 'd':
									replace = art.Date.Month.ToString() + "/" + art.Date.Day.ToString() + "/" + art.Date.Year.ToString();
									break;
								case 'D':
									replace = System.DateTime.Now.Month.ToString() + "/" + System.DateTime.Now.Day.ToString() + "/" + System.DateTime.Now.Year.ToString();
									break;
								case 'i':
									replace = art.ImportFile;
									break;
								case 'y':
									System.Text.RegularExpressions.Match match2 = 
										regex.Match(art.ImportFile);
									if (art.ImportFile.ToLower().StartsWith("msgidlist"))
									{
										match2 = 
											regex2.Match(art.ImportFile);
									}

									if( match2 != null &&
										match2.Value.Length > 0)
									{
										replace = art.ImportFile.Replace(match2.Value, "");
									}
									else
									{
										replace = art.ImportFile;
									}

									replace = replace.Replace("_", " ");
									break;
								case 'z':
									System.Text.RegularExpressions.Match match = 
										regex.Match(art.ImportFile);
									if (art.ImportFile.ToLower().StartsWith("msgidlist"))
									{
										match = 
											regex2.Match(art.ImportFile);
									}

									if( match != null &&
                                        match.Value.Length > 0)
                                    {
										replace = art.ImportFile.Replace(match.Value, "");
									}
									else
									{
										replace = art.ImportFile;
									}
									break;
								default:
									break;
							}
							
							str = str.Substring(0, index - 1) + replace + ReplaceVariables(str.Substring(index + 1), art);
						}
					}
				}
			}
			while(str.StartsWith("\\"))
				str = str.Remove(0, 1);

			while(str.EndsWith("\\"))
				str = str.Remove(str.Length - 2, 1);
			return str;
		}


		private static ManagementObjectSearcher GetDriveInfo(string name)
		{
			
			SelectQuery query = new SelectQuery("Win32_LogicalDisk", "Name='" + name + "'");

			ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

			return searcher;
		}

		public enum DecodeStatus { Decoded, FailedCRC, FailedNothingToDecode };

		public static DecodeStatus DecodeArticle( Article article)
		{
			System.IO.FileStream output = null;
			System.IO.StreamReader input = null;

			string line;
			string decoder = "";
			string sdecoder = "";

			string outputfile = "";
			long outputsize = -1;

			bool crcfailed = false;
			crc32 crc = new crc32();

			int decodedsegments = 0;

			foreach( Segment segment in article.Segments)
			{
				// Check if the file exists, if not, skip the segment
				if( !System.IO.File.Exists( System.IO.Path.GetFullPath(Global.m_CacheDirectory + segment.ArticleID)))
					continue;

				input = new System.IO.StreamReader( System.IO.Path.GetFullPath(Global.m_CacheDirectory + segment.ArticleID), System.Text.Encoding.GetEncoding("iso-8859-1"));

				// If uudecode is used, each file is automaticly a new segment
				if( decoder == "uudecode" || (decoder == "mime" && sdecoder == "base64"))
					decodedsegments ++;
			
				line = input.ReadLine();
				while( line != null)
				{
					if( decoder == "mime")
					{
						if(	line.StartsWith( "Content-Transfer-Encoding:"))
							sdecoder = line.Remove(0, 27);

						if( line.StartsWith("--=") || line.StartsWith("Content-Type:"))
						{
							decoder = "";
							sdecoder = "";

							outputfile = "";

							output.Close();
							output = null;
						}

						// Perhaps get filename out of this, but its also in the content type
						if( line.StartsWith( "Content-Disposition:"))
							line = "";

						if( sdecoder == "base64")
						{
							if( line.Length % 4 > 0)
								line = "";

							if( line != "")
							{
								byte[] buffer;
								buffer = Convert.FromBase64String( line);

								output.Write( buffer, 0, buffer.Length);
							}
						}
					}

					if( decoder == "uudecode")
					{
						if( line != "" && line != "end")
						{
							char[] buffer = line.ToCharArray();
							if( uudecode_checkline( buffer))
							{
								int p = 0;
								int n = 0;
								byte ch;

								n = uudecode_dec( buffer[p]);
								for( ++p; n > 0; p += 4, n -= 3)
								{
									if (n >= 3) 
									{
										// Error ?
										if (!(uudecode_is_dec(buffer[p]) && uudecode_is_dec(buffer[p + 1]) && uudecode_is_dec(buffer[p + 2]) && uudecode_is_dec(buffer[p + 3])))
											throw new Exception( "33");

										ch = (byte)(uudecode_dec(buffer[p+0]) << 2 | uudecode_dec(buffer[p+1]) >> 4);
										output.WriteByte(ch);
										ch = (byte)(uudecode_dec(buffer[p+1]) << 4 | uudecode_dec(buffer[p+2]) >> 2);
										output.WriteByte(ch);
										ch = (byte)(uudecode_dec(buffer[p+2]) << 6 | uudecode_dec(buffer[p+3]));
										output.WriteByte(ch);

									}
									else 
									{
										if (n >= 1) 
										{
											if (!(uudecode_is_dec(buffer[p]) && uudecode_is_dec(buffer[p+1])))
												throw new Exception( "34");

											ch = (byte)(uudecode_dec(buffer[p+0]) << 2 | uudecode_dec(buffer[p+1]) >> 4);
											output.WriteByte(ch);
										}
										if (n >= 2) 
										{
											if (!(uudecode_is_dec(buffer[p+1]) && uudecode_is_dec(buffer[p+2])))
												throw new Exception( "35");

											ch = (byte)(uudecode_dec(buffer[p+1]) << 4 | uudecode_dec(buffer[p+2]) >> 2);
											output.WriteByte(ch);
										}
										if (n >= 3) 
										{
											if (!(uudecode_is_dec(buffer[p+2]) && uudecode_is_dec(buffer[p+3])))
												throw new Exception( "36");

											ch = (byte)(uudecode_dec(buffer[p+2]) << 6 | uudecode_dec(buffer[p+3]));
											output.WriteByte(ch);
										}
									}
								}
							}
						}

						if( line == "end")
						{
							decoder = "";
							outputfile = "";

							output.Close();
							output = null;
						}
					}

					if( decoder == "yenc")
					{
						if( line.StartsWith( "=ypart "))
						{
							// Part description
							string[] ypart = line.Split( " ".ToCharArray());
							foreach( string s in ypart)
							{
								if( s.StartsWith( "begin"))
								{
									output.Seek( long.Parse( s.Remove(0, 6))-1, System.IO.SeekOrigin.Begin);
								}
							}
						}
						else
						{
							if( line.StartsWith( "=yend "))
							{
								// End of the Yenc part, do CRC check
								decoder = "";

								string[] yend = line.Split( " ".ToCharArray());
								foreach( string s in yend)
								{
									if( s.StartsWith( "pcrc32"))
									{
										long opcrc = Convert.ToInt64(s.Remove(0, 7), 16);
										long cpcrc = crc.EndByteCRC();
										
										if( opcrc != cpcrc)
											crcfailed = true;
									}
								}
								decodedsegments ++;

								if( outputsize == output.Length)
									outputsize = -1;

								if( outputsize == -1)
								{
									output.Close();
									output = null;

									outputfile = "";
								}
							}
							else
							{
								// Yenc Encoded part
								bool escape = false;
								foreach( char c in line.ToCharArray())
								{
									if( c == '=' && !escape)
									{
										escape = true;
									}
									else
									{
										byte nc = (byte)c;
										if( escape)
										{
											nc = (byte)(nc-64);
											escape = false;
										}

										nc = (byte)(nc-42);
										output.WriteByte(nc);
										crc.AddByteCRC(nc);
									}
								}
							}
						}
					}

					if( decoder == "")
					{
						if( line.StartsWith( "=ybegin "))
						{
							int c;
							decoder = "yenc";
							crc.StartByteCRC();
							//rebuild correctly missformed spaces in this line, but not the filename
							c = line.IndexOf( "name");
							string ybegin = line.Substring(0, c-1);
							string name = line.Substring(c+5);
							line = ybegin.Replace(" ",""); //strip spaces
							ybegin = line.Replace("part="," part=");//add spaces where nedded
							line = ybegin.Replace("line="," line=");
							ybegin = line.Replace("size="," size=");
							line = ybegin + " name=" + name;
							
							// Check if its a valid ybegin line, as per 1.2 line, size and name have to be present
							if( line.IndexOf("line=") != -1 && line.IndexOf("size=") != -1 && line.IndexOf("name=") != -1)
							{
								int b, e;
								b = line.IndexOf( "size=");
								e = line.IndexOf( " ", b);
								outputsize = long.Parse( line.Substring(b+5, e-b-5));
								b = line.IndexOf( "name=");
								if( outputfile != line.Substring(b+5))
								{
									outputfile = line.Substring(b+5);
									if( article.Filename == "")
										article.Filename = outputfile;
									//why add second filename?
									//article.Filename = article.Filename + outputfile;

									string outputdir = GetDirectory(article);
									
									try
									{
										if( !System.IO.Directory.Exists( System.IO.Path.GetFullPath(outputdir)))
											System.IO.Directory.CreateDirectory( System.IO.Path.GetFullPath(outputdir));
									}
									catch( Exception ex)
									{
										frmMain.LogWriteError( "Unable to create directory [" + outputdir + "]");
										throw(ex);
									}

									if( output != null)
									{
										output.Close();
										output = null;
									}
									
									string outputdrive = outputdir.Substring(0,2);
									long  size = article.Size;
									
                                    ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid='" + outputdrive + "'");
                                    try
                                    {
                                        if (size > Convert.ToInt64(disk[DiskProperties.FreeSpace.ToString()].ToString()))
                                        {
                                            frmMain.LogWriteError("Unable to create file : no space on drive " + outputdrive);
                                            input.Close();
                                            input = null;
                                            return DecodeStatus.FailedNothingToDecode;
                                        }
                                        else
                                        {
                                            output = new System.IO.FileStream(System.IO.Path.GetFullPath(outputdir) + "\\" + outputfile, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write, System.IO.FileShare.None, 1024 * 1024);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        frmMain.LogWriteError("Unable to create file : " + ex.Message);
                                        input.Close();
                                        input = null;
                                        return DecodeStatus.FailedNothingToDecode;
                                    }
									
								}
							}
						}

						if( line.StartsWith( "begin 644 "))
						{
							decodedsegments ++;

							decoder = "uudecode";

							outputfile = line.Remove(0, 10);
							if( article.Filename == "")
								article.Filename = outputfile;
							//why add second file name?
							//article.Filename = article.Filename + outputfile;

							string outputdir = GetDirectory(article);

							try
							{
								if( !System.IO.Directory.Exists( System.IO.Path.GetFullPath(outputdir)))
									System.IO.Directory.CreateDirectory( System.IO.Path.GetFullPath(outputdir));
							}
							catch( Exception ex)
							{
								frmMain.LogWriteError( "Unable to create directory [" + outputdir + "]");
								throw(ex);
							}

							if( output != null)
							{
								output.Close();
								output = null;
							}

							string outputdrive = outputdir.Substring(0,2);
							long  size = article.Size;
                            ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid='" + outputdrive + "'");
                            try
                            {
                                if (size > Convert.ToInt64(disk[DiskProperties.FreeSpace.ToString()].ToString()))
                                {
                                    frmMain.LogWriteError("Unable to create file : no space on drive " + outputdrive);
                                    input.Close();
                                    input = null;
                                    return DecodeStatus.FailedNothingToDecode;
                                }
                                else
                                {
                                    output = new System.IO.FileStream(System.IO.Path.GetFullPath(outputdir) + "\\" + outputfile, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write, System.IO.FileShare.None, 1024 * 1024);
                                }
                            }
                            catch (Exception ex)
                            {
                                frmMain.LogWriteError("Unable to create file : " + ex.Message);
                                input.Close();
                                input = null;
                                return DecodeStatus.FailedNothingToDecode;
                            }
						}

						if( line.StartsWith( "Content-Type: application/octet-stream;"))
						{
							decodedsegments ++;

							decoder = "mime";
							sdecoder = "";

							outputfile = line.Substring( line.IndexOf( "name=") + 5);
							if( outputfile[0] == '\"' && outputfile[outputfile.Length - 1] == '\"')
								outputfile = outputfile.Substring(1, outputfile.Length - 2);

							if( outputfile[0] == '\'' && outputfile[outputfile.Length - 1] == '\'')
								outputfile = outputfile.Substring(1, outputfile.Length - 2);

							if( article.Filename == "")
								article.Filename = outputfile;
							//why add second file name?
							//article.Filename = article.Filename + outputfile;

							string outputdir = GetDirectory(article);

							try
							{
								if( !System.IO.Directory.Exists( System.IO.Path.GetFullPath(outputdir)))
									System.IO.Directory.CreateDirectory( System.IO.Path.GetFullPath(outputdir));
							}
							catch( Exception ex)
							{
								frmMain.LogWriteError( "Unable to create directory [" + outputdir + "]");
								throw(ex);
							}

							if( output != null)
							{
								output.Close();
								output = null;
							}
							string outputdrive = outputdir.Substring(0,2);
							long  size = article.Size;                         
                            ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid='" + outputdrive + "'");
                            try
							{
								if (size > Convert.ToInt64(disk[DiskProperties.FreeSpace.ToString()].ToString()))
								{
									frmMain.LogWriteError( "Unable to create file : no space on drive " + outputdrive);
									input.Close();
									input = null;
									return DecodeStatus.FailedNothingToDecode;
								}
								else
								{
								output = new System.IO.FileStream( System.IO.Path.GetFullPath(outputdir) + "\\" + outputfile, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write, System.IO.FileShare.None, 1024*1024);
								}
							}
                            catch( Exception ex ) {
                                frmMain.LogWriteError( "Unable to create file : " + ex.Message);
								input.Close();
								input = null;
								return DecodeStatus.FailedNothingToDecode;
                            }
						}
					}

					line = input.ReadLine();
				}

				input.Close();
				input = null;

				try
				{
					if( output != null)
						output.Flush();
				}
				catch( Exception ex)
				{
					frmMain.LogWriteError("pb during Flushing on disk");
					throw(ex);
				}
			}

			if( output != null)
			{
				output.Close();
				output = null;
			}

			// Changed the behaviour of deleting segments, delete all segments
			// unless nothing got decoded
			if( decodedsegments != 0)
			{
				// Pretty sure everything went ok, deleting partial files...
				foreach( Segment segment in article.Segments)
/*					if( System.IO.File.Exists( System.IO.Path.GetFullPath("Cache\\" + segment.ArticleID)))
						System.IO.File.Delete( System.IO.Path.GetFullPath("Cache\\" + segment.ArticleID));
*/
					if( System.IO.File.Exists(System.IO.Path.GetFullPath(Global.m_CacheDirectory + segment.ArticleID)))
						System.IO.File.Delete(System.IO.Path.GetFullPath(Global.m_CacheDirectory + segment.ArticleID));
			}

			if( crcfailed)
				return DecodeStatus.FailedCRC;

			if( decodedsegments == 0)
				return DecodeStatus.FailedNothingToDecode;

			return DecodeStatus.Decoded;
		}
	}
}
