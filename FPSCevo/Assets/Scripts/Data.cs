using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace FPSCevo
{
    public class Data : MonoBehaviour
    {
        public static void SaveProfile (ProfileData t_profile)
        {
            try
            {
                string path = Application.persistentDataPath + "/profile.dt";

                if (File.Exists(path)) File.Delete(path); //ismi de değiştirilebilir başka yere de taşınabilir backuplı çalışmak istiyorsan

                FileStream file = File.Create(path);

                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(file, t_profile);
                file.Close();

                Debug.Log("Save işlemi başarılı");
            }
            catch
            {
                Debug.Log("Save işlemi başarısız");
            }
        }

        public static ProfileData LoadProfile()
        {
            ProfileData returnn = new ProfileData();
            try
            {
                string path = Application.persistentDataPath + "/profile.dt";

                if (File.Exists(path))
                {
                    FileStream file = File.Open(path, FileMode.Open);
                    BinaryFormatter bf = new BinaryFormatter();
                    returnn = (ProfileData)bf.Deserialize(file);

                    Debug.Log("Load işlemi başarılı");
                }
            }
            catch
            {
                Debug.Log("Load işlemi başarısız");
            }

            return returnn;
        }
    }
}

