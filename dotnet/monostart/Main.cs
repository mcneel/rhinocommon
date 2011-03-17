using System;

namespace monostart
{
  class MainClass
  {
    public static void Main (string[] args)
    {
      // This application does absolutely nothing!!
      // It's sole purpose is for initalization of embedded mono in Mac Rhino
      // Embedded mono needs to execute an application with a Main function in order
      // to get some of the settings on the AppDomain properly set up
    }
  }
}
