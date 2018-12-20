using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PS {
    public class StringTuple : Tuple<string, string> {
        public StringTuple( string i1, string i2 ) : base( i1.ToLower(), i2.ToLower() ) { }
        public override string ToString() {
            return "" + Item1 + " " + Item2;
        }
    }

    public partial class MainWindow : Window {
        public HashSet<string> Cs = new HashSet<string>();
        public HashSet<string> Vs = new HashSet<string>();

        public HashSet<StringTuple> CCs = new HashSet<StringTuple>();
        public HashSet<StringTuple> VVs = new HashSet<StringTuple>();
        public HashSet<StringTuple> CVs = new HashSet<StringTuple>();
        public HashSet<StringTuple> VCs = new HashSet<StringTuple>();

        public HashSet<StringTuple> notCCs = new HashSet<StringTuple>();
        public HashSet<StringTuple> notVVs = new HashSet<StringTuple>();
        public HashSet<StringTuple> notCVs = new HashSet<StringTuple>();
        public HashSet<StringTuple> notVCs = new HashSet<StringTuple>();

        public MainWindow() {
            InitializeComponent();
            Closing += Window_Closing;
            DataContext = this;
        }

        private void Window_Closing( object sender, CancelEventArgs e ) => Properties.Settings.Default.Save();

        private void Open_Button_Click( object sender, RoutedEventArgs e ) {
            OpenFileDialog openFileDialog = new OpenFileDialog {
                InitialDirectory = Properties.Settings.Default.FileDialogDir,
                Filter = "Text files (*.txt)|*.txt|index.csv|*.csv|All files (*.*)|*.*"
            };
            if ( openFileDialog.ShowDialog() == true ) {
                string fn = openFileDialog.FileName;
                FileName.Text = fn;
                Properties.Settings.Default.FileDialogDir = new DirectoryInfo( fn ).Name;
                SetContent( File.ReadAllLines( fn ) );
            }
        }

        private void ListView_SelectionChanged( object sender, SelectionChangedEventArgs e ) {
            if ( e.AddedItems.Count == 0 ) {
                return;
            }

            ChangeSearch( e.AddedItems[0] as string );
        }

        private void Clipboard_Click( object sender, RoutedEventArgs e ) {
            string s = "```\nhas:\n"
                + LineToString( hasCCVV.Items )
                + LineToString( hasCV.Items )
                + LineToString( hasVC.Items )
                + "\nmissing:\n"
                + LineToString( misCCVV.Items )
                + LineToString( misCV.Items )
                + LineToString( misVC.Items )
                + "```";
            Clipboard.SetData( DataFormats.Text, s );
        }

        private string LineToString( ItemCollection lv ) {
            if ( lv.Count != 0 ) {
                return string.Join( "  ", lv.OfType<StringTuple>().Select( item => item.ToString() ).ToList() ) + '\n';
            }
            return "";
        }

        private string lastSearch = "";
        private void ChangeSearch( string s ) {
            if ( s == "" )
                return;
            s = s.ToLower();
            Func<StringTuple, bool> f = (w => w.Item1 == s || w.Item2 == s);

            hasCCVV.ItemsSource = (IsVowel(s)?VVs:CCs).OrderBy( i => i ).Where( f );
            hasCV.ItemsSource = CVs.OrderBy( i => i ).Where( f );
            hasVC.ItemsSource = VCs.OrderBy( i => i ).Where( f );
            misCCVV.ItemsSource = (IsVowel( s ) ? notVVs : notCCs).OrderBy( i => i ).Where( f );
            misCV.ItemsSource = notCVs.OrderBy( i => i ).Where( f );
            misVC.ItemsSource = notVCs.OrderBy( i => i ).Where( f );

            lastSearch = s;
        }

        private void SetContent( string[] lines ) {
            Cs.Clear();
            Vs.Clear();

            foreach ( string line in lines ) {
                string[] words = Parse_Line( line.ToLower() );

                foreach ( StringTuple pair in words.Zip( words.Skip( 1 ), ( a, b ) => new StringTuple( a, b ) ) ) {
                    AddToProperSet( pair );
                }
            }

            // set the missing vars // notCCs, notVVs, etc
            FindMissings();

            // set the observable Cs and Vs
            CList.ItemsSource = Cs.OrderBy( i => i );
            VList.ItemsSource = Vs.OrderBy( i => i );

            // set the counts
            TotalSounds.Text = "" + (Cs.Count + Vs.Count);
            TotalPairs.Text = "" + (CCs.Count + VVs.Count + CVs.Count + VCs.Count);
            TotalPotentialPairs.Text = "" + (2 * Cs.Count * Vs.Count + Cs.Count * Cs.Count + Vs.Count * Vs.Count);
        }

        private async void FindMissings() => await Task.Run( new Action( () => {
            notVVs.Clear();
            notVCs.Clear();
            notCVs.Clear();
            notCCs.Clear();
            foreach ( string v1 in Vs ) {
                foreach ( string v2 in Vs ) {
                    notVVs.Add( new StringTuple( v1, v2 ) );
                }
                foreach ( string c2 in Cs ) {
                    notVCs.Add( new StringTuple( v1, c2 ) );
                }
            }
            foreach ( string c1 in Cs ) {
                foreach ( string v2 in Vs ) {
                    notCVs.Add( new StringTuple( c1, v2 ) );
                }
                foreach ( string c2 in Cs ) {
                    notCCs.Add( new StringTuple( c1, c2 ) );
                }
            }

            notVVs.RemoveWhere( x => VVs.Contains( x ) );
            notVCs.RemoveWhere( x => VCs.Contains( x ) );
            notCVs.RemoveWhere( x => CVs.Contains( x ) );
            notCCs.RemoveWhere( x => CCs.Contains( x ) );
        } ) );

        private void AddToProperSet( StringTuple pair ) {
            bool i1v = IsVowel( pair.Item1 );
            bool i2v = IsVowel( pair.Item2 );

            // add to Vs or Cs
            (i1v ? Vs : Cs).Add( pair.Item1 );
            (i2v ? Vs : Cs).Add( pair.Item2 );

            // add to the proper list (VVs, CCs, CVs, VCs)
            (i1v ?
                (i2v ? VVs : VCs) : // item1 is V
                (i2v ? CVs : CCs) // item1 is C
                ).Add( pair );
        }

        private static bool IsVowel( string s ) {
            return "aeiou".Contains( s[0] );
        }

        private string[] Parse_Line( string line ) {
            // types of lines you need to handle. you need to extract the PPs
            /* Symbol Key:
             * PP		Arpasing Phoneme (or any text really)
             * _		Either a underscore or a space (program handles them the same)
             * ,		Either a comma or a tab (yep. you guessed it. handled the same)
             * ?		Ignored random characters
             * 
             * PP_PP_PP
             * ?????,PP_PP_PP
             * ?????(PP_PP_PP)
             */
            if ( line.Contains( '\t' ) || line.Contains( ',' ) ) {
                // get all after the last comma or tab
                line = line.Split( "\t,".ToCharArray() ).Last();
            }

            if ( line.Contains( '(' ) ) { // get between the parens
                line = line.Split( "()".ToCharArray() )[1];
            }

            line = line.Replace( "-", string.Empty ); // dashes should always be ignored

            // split on spaces or underscores
            // remove less than 7
            // remove startswith number
            return line.Split( " _".ToCharArray(), StringSplitOptions.RemoveEmptyEntries ).Where( x => x.Length < 7 && !"0123456789".Contains(x[0]) ).ToArray();
        }
    }
}
