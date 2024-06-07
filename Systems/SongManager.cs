using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace SynthSharp;

public class SongManager
{
    private List<Song> songs;
    private int index = 0;

    public SongManager(List<Song> songs)
    {
        songs = songs.OrderBy(x => Guid.NewGuid()).ToList();
        this.songs = songs;
        MediaPlayer.IsRepeating = false;
        Play();
    }

    private void Play()
    {
        MediaPlayer.Play(songs[index]);
    }

    public void Stop()
    {
        MediaPlayer.Stop();
    }

    public void Update()
    {
        if (MediaPlayer.State == MediaState.Stopped)
        {
            index++;
            if (index >= songs.Count)
            {
                index = 0;
            }
            Play();
        }
    }

    public static void PlaySpecific(object audio)
    {
        if (audio is Song song)
        {
            MediaPlayer.Play(song);
        }
        else if (audio is SoundEffect soundEffect)
        {
            soundEffect.Play();
        }
    }
}