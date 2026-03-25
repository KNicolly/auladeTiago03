using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
   public static AudioManager Instance;

   private AudioSource audioSource;
   private List<AudioSource> activeSources;
   
   
   
   private void Awake()
   {
      if (Instance == null)
      {

         Instance = this;
         DontDestroyOnLoad(gameObject);
      }
      else
      {
         Destroy(gameObject);

      }
   }
   
   //Funções de gerenciamento de audio
   public void PlaySound(AudioClip clip)
   {
      
      systemSource.clip = clip;
   }

   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
} 

