﻿using System;

namespace NBB.SQLStreamStore.Internal
{
    public interface ISerDes
    {
        T Deserialize<T>(string data);
        object Deserialize(string data, Type type);

        string Serialize(object obj);
    }
}
