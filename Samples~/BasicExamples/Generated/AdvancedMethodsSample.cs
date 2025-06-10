using System;
using System.Collections.Generic;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generated
{
    public class AdvancedMethodDemo
    {
        public void SimpleVoidMethod()
        {
            Console.WriteLine("This is a simple void method");
        }

        public int Add(int a, int b)
        {
            return a + b;
        }

        public static List<string> ProcessData(string[] data)
        {
            var result = new List<string>(); foreach  ( var  item  in  data ) { if  ( string . IsNullOrEmpty ( item ) ) continue ;  var  processed  =  item . Trim ( ) . ToUpper ( ) ;  result . Add ( processed ) ;  } return  result ; 
        }

        private bool ValidateInput(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Input is empty");
                return false;
            } if  ( input . Length < 3 ) { Console . WriteLine ( "Input is too short" ) ;  return  false ;  } return  true ; 

        }

        public string ProcessInput(string input)
        {
            if (!ValidateInput(input))
                return string.Empty; var  data  =  input . Split ( ',' ) ;  var  processed  =  ProcessData ( data ) ;  return  string . Join ( "-" ,  processed ) ; 
        }

        public string FormatText(string text, bool uppercase = true, bool trim = true)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty; var  result  =  text ;  if  ( trim ) result  =  result . Trim ( ) ;  if  ( uppercase ) result  =  result . ToUpper ( ) ;  return  result ; 
        }
    }
}