using System;

namespace UnrealFlow{

  static class DateExtentions {

    public static long ToUnixSeconds( this DateTime dateTime ) {
      return ( (DateTimeOffset)dateTime ).ToUnixTimeSeconds();
    }
  }
}