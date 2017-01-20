package com.unity3d.plugin.downloader;

import com.google.android.vending.expansion.downloader.impl.DownloaderService;

public class UnityDownloaderService extends DownloaderService {
    // stuff for LVL -- MODIFIED FROM C# SCRIPTS!
    static String BASE64_PUBLIC_KEY = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAqkj6vpayjmdPG1gSIlUwbIdVyWeh2Wilw94C7cvT0f4Y1dALXTzB0lr5DzCPFMslAgxpuZjbP/8b1LPwg5pasn/v1G4gKSEjR1OyDKSz/DgqVRLOuvya8q95tdW3WTmKxkCYpYJv6TuXg7dejNJPG2/HcXpwi3HXw6p9zGK5GITloU2rqxS3oNGccNfbXVJy4gqGFGGnxL/HGElFq6o2LxVyfphxW4+vketTOiHEixO7+w/Hj6ALmD3rWXEikSb9PfEY8ULS4ZMP5H34jd5ijfpaK9AQAqbOysgfbVaYs/CNF7Fb6zVVGNf6YtY3E+eWm9lTJr5nyeaCJO/QWw9GfwIDAQAB";
    // used by the preference obfuscater
    static byte[] SALT = new byte[] {
            1, 43, -12, -1, 54, 98,
            -100, -12, 43, 2, -8, -4, 9, 5, -106, -108, -33, 45, -1, 84
    };

    /**
     * This public key comes from your Android Market publisher account, and it
     * used by the LVL to validate responses from Market on your behalf.
     */
    @Override
    public String getPublicKey() {
        return BASE64_PUBLIC_KEY;
    }

    /**
     * This is used by the preference obfuscater to make sure that your
     * obfuscated preferences are different than the ones used by other
     * applications.
     */
    @Override
    public byte[] getSALT() {
        return SALT;
    }

    /**
     * Fill this in with the class name for your alarm receiver. We do this
     * because receivers must be unique across all of Android (it's a good idea
     * to make sure that your receiver is in your unique package)
     */
    @Override
    public String getAlarmReceiverClassName() {
        return UnityAlarmReceiver.class.getName();
    }

}
