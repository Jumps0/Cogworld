using System;
using System.Collections;
using System.Collections.Generic;

public interface iDataService
{
    bool SaveData<T>(string relativePath, List<T> data);
    bool SaveData<T>(string relativePath, T data);

    T LoadData<T>(string relativePath);
}
