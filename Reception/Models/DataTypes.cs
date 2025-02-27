﻿namespace Reception.Models;

public static class DataTypes
{
    public enum Dimension {
        THUMBNAIL,
        MEDIUM,
        SOURCE
    }
    public enum Method {
        UNKNOWN,
        HEAD,
        GET,
        POST,
        PUT,
        PATCH,
        DELETE
    }
    public enum Severity {
        TRACE,
        DEBUG,
        INFORMATION,
        SUSPICIOUS,
        WARNING,
        ERROR,
        CRITICAL
    }
    public enum Source {
        INTERNAL,
        EXTERNAL
    }
}