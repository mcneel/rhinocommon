using System;
// don't wrap right away. this one is hard
//public class ON_UserData { }
//public class ON_UnknownUserData : ON_UserData { }
//public class ON_UserStringList : ON_UserData { }
//public class ON_UserDataHolder { }

namespace Rhino.DocObjects
{
  public abstract class UserData
  {
    private UserData() { } //for now

    /// <summary>
    /// Expert user tool that copies user data that has a positive 
    /// CopyCount from the source object to a destination object.
    /// Generally speaking you don't need to use Copy().
    /// Simply rely things like copy constructors to do the right thing
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    public static void Copy(Rhino.Runtime.CommonObject source, Rhino.Runtime.CommonObject destination)
    {
      IntPtr pConstSource = source.ConstPointer();
      IntPtr pDestination = destination.NonConstPointer();
      UnsafeNativeMethods.ON_Object_CopyUserData(pConstSource, pDestination);
    }

    /// <summary>
    /// Moves the user data from objectWithUserData to a temporary data storage
    /// identifierd by the return Guid.  When MoveUserDataFrom returns, the
    /// objectWithUserData will not have any user data.
    /// </summary>
    /// <param name="objectWithUserData">Object with user data attached</param>
    /// <returns>
    /// Guid identifier for storage of UserData that is held in a temporary list
    /// by this class. This function should be used in conjunction with MoveUserDataTo
    /// to transfer the user data to a different object.
    /// Returns Guid.Empty if there was no user data to transfer
    /// </returns>
    public static Guid MoveUserDataFrom(Rhino.Runtime.CommonObject objectWithUserData)
    {
      Guid id = Guid.NewGuid();
      IntPtr pConstObject = objectWithUserData.ConstPointer();
      if (UnsafeNativeMethods.ON_UserDataHolder_MoveUserDataFrom(id, pConstObject))
        return id;
      return Guid.Empty;
    }
    
    /// <summary>
    /// See MoverUserDataFrom
    /// </summary>
    /// <param name="objectToGetUserData"></param>
    /// <param name="id"></param>
    /// <param name="append"></param>
    public static void MoveUserDataTo(Rhino.Runtime.CommonObject objectToGetUserData, Guid id, bool append)
    {
      if (id != Guid.Empty)
      {
        IntPtr pConstObject = objectToGetUserData.ConstPointer();
        UnsafeNativeMethods.ON_UserDataHolder_MoveUserDataTo(id, pConstObject, append);
      }
    }
  }
}