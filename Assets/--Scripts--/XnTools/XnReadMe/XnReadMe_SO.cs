using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace XnTools {

	[CreateAssetMenu( fileName = "---ReadMe---", menuName = "ScriptableObjects/ReadMe", order = 0 )]
	public class XnReadMe_SO : ScriptableObject {
		public Texture2D icon = null;
		// Assets/___Scripts/XnTools/ProjectInfo Icons/ReadMe.png

		[HideInInspector]
		public float iconMaxWidth = 128f;


		[TextArea( 1, 10 )]
		public string projectName      = DEFAULT_PROJ_NAME;
		public string author           = DEFAULT_AUTHOR;
		public string modificationDate = currentDate;

		public Section[] sections = readMeDefaultSections;

		[HideInInspector]
		public bool showReadMeEditor = false;

		const string DEFAULT_PROJ_NAME = "Replace this Project Name";
		const string DEFAULT_AUTHOR    = "Replace this with your team members' names";
		public void ResetReadMeToDefaults() {
			projectName = DEFAULT_PROJ_NAME;
			author = DEFAULT_AUTHOR;
			sections = readMeDefaultSections;
		}

		public string ToMarkDownString() {
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.AppendLine( $"# **{projectName.Replace( '\n', ' ' )}** - ReadMe File\n" );
			sb.AppendLine( $"#### Author: *{author}*\n" );
			sb.AppendLine( $"##### Modified: *{modificationDate}*\n" );
			sb.AppendLine( "\n<br>\n" );
			foreach ( Section sec in sections ) {
				sb.AppendLine( sec.ToMarkDownString() );
			}

			return sb.ToString();
		}

		static public string currentDate {
			get {
				// Update the date
				DateTime now = DateTime.Now;
				return $"{now.Year:0000}-{now.Month:00}-{now.Day:00}";
			}
		}

		static private Section[] readMeDefaultSections {
			get {
				List<Section> sections = new List<Section>();
				Section sec;
				
				// Itch.io Link
				sec = new Section();
				sec.heading = "1. itch.io link for your game:";
				sec.text = "<i>(Just paste the link here.)</i>";
				sections.Add( sec );
				
				// Description
				sec = new Section();
				sec.heading = "2. Description of your game:";
				sec.text = "<i>(Give us a brief description of the changes you have made.)</i>";
				sections.Add( sec );

				// Controls
				sec = new Section();
				sec.heading = "3. Are there any changes to the controls of your game? How do we play?";
				sec.text = "<i>(Did you add or remove any controls from the ones that were included?)</i>";
				sections.Add( sec );

				// Creative Additions
				sec = new Section();
				sec.heading = "4. What creative remix changes did you make? How can we find them?";
				sec.text = "<i>(If you don't tell us how to experience them, we could easily miss them.)</i>";
				sections.Add( sec );

				// Assets
				sec = new Section();
				sec.heading = "5. Any assets used that you didn't create yourself?";
				sec.text = "<i>(art, music, etc. Just tell us where you got it, link it here)</i>";
				sections.Add( sec );

				// Help - Person
				sec = new Section();
				sec.heading = "6. Did you receive help from anyone outside this class or from anyone" +
				              " in this class that is not in a group with you?";
				sec.text = "<i>(list their names and what they helped with)</i>";
				sections.Add( sec );

				// Help - Websites and Tutorials
				sec = new Section();
				sec.heading = "7. Did you get help from any online websites, videos, or tutorials?";
				sec.text = "<i>(link them here)</i>";
				sections.Add( sec );

				// Help - AI Assistants
				sec = new Section();
				sec.heading = "8. Did you get help from any Extractive AI Code or Art Assistants?";
				sec.text = "<i>(Including things like Chat-GPT, Copilot, etc. If it is code, tell us which .cs file" +
				           " to look in for the citation and describe what you learned)</i>";
				sections.Add( sec );

				// Trouble
				sec = new Section();
				sec.heading = "9. What trouble did you have with this project?";
				sec.text = "<i>(Even if you didn't complete the project, you can still get partial" +
				           " credit if you tell us about why it's incomplete.)</i>";
				sections.Add( sec );

				// Anything Else
				sec = new Section();
				sec.heading = "10. Is there anything else we should know?";
				sections.Add( sec );

				return sections.ToArray();
			}
		}


		[Serializable]
		public class Section {
			[TextArea( 1, 10 )]
			public string heading, text; //, linkText, url;

			static public Section demo {
				get {
					Section sec = new Section();
					sec.heading = "Section Heading";
					sec.text =
						"<b>Section</b> <i>text</i>.\nHold <b>option/alt</b> and click this to Show Default Inspector.";
					return sec;
				}
			}

			public string ToMarkDownString() {
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				sb.AppendLine(
					$"**{heading}**   " ); // Note: The extra spaces at the end of the line tell MarkDown to add <br>
				sb.AppendLine( "\n> &nbsp;" );
				if ( String.IsNullOrEmpty( text ) ) {
					sb.AppendLine( $">*No answer given.*" );
					// if ( url == "" ) {
					// 	sb.AppendLine( $">*No answer given.*" );
					// }
				} else {
					if ( sb == null || text == null ) {
						Debug.Log( "BREAK" );
					} else {
						sb.AppendLine( $">{text.Replace( "\n", "    \n> " )}   " );
					}
				}

				// Actually, this whole link extraction isn't needed in GitLab or GitHub
				// because they extract URLs automatically! - JGB 2024-08-28
				/*
				// Extract the URLs in the text and make them individual buttons
				List<string> urlList = ExtractUrls( text );
				if ( urlList.Count > 0 ) {
					sb.AppendLine( $"   \n>   \n> ***Links in this section***   " );
				}
				foreach (string url in urlList) {
					sb.AppendLine( $">[{url}]({url})    " );
				}

				// if ( url != "" ) {
				// 	if ( linkText != "" ) {
				// 		sb.AppendLine( $">[{linkText}]({url})" );
				// 	} else {
				// 		sb.AppendLine( $"><{url}>" );
				// 	}
				// }
				*/
				sb.AppendLine( "> &nbsp;\n \n" );
				return sb.ToString();
			}

			public List<string> ExtractUrls( string str ) {
				var urls = new List<string>();
				if ( string.IsNullOrEmpty( str ) ) {
					return urls;
				}

				var regex = new Regex( @"https?://[^\s]+", RegexOptions.Compiled | RegexOptions.IgnoreCase );
				var matches = regex.Matches( str );

				foreach ( Match match in matches ) {
					urls.Add( match.Value );
				}

				return urls;
			}
		}


	}
}