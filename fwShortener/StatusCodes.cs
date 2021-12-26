namespace fwShortener;

public static class StatusCodes
{
    private static readonly String[][] s_HTTPStatusDescriptions = new String[][]
    {
        null,
    
        new String[]
        { 
            /* 100 */"Continue",
            /* 101 */ "Switching Protocols",
            /* 102 */ "Processing"
        },
    
        new String[]
        { 
            /* 200 */"OK",
            /* 201 */ "Created",
            /* 202 */ "Accepted",
            /* 203 */ "Non-Authoritative Information",
            /* 204 */ "No Content",
            /* 205 */ "Reset Content",
            /* 206 */ "Partial Content",
            /* 207 */ "Multi-Status"
        },
    
        new String[]
        { 
            /* 300 */"Multiple Choices",
            /* 301 */ "Moved Permanently",
            /* 302 */ "Found",
            /* 303 */ "See Other",
            /* 304 */ "Not Modified",
            /* 305 */ "Use Proxy",
            /* 306 */ String.Empty,
            /* 307 */ "Temporary Redirect"
        },
    
        new String[]
        { 
            /* 400 */"Bad Request",
            /* 401 */ "Unauthorized",
            /* 402 */ "Payment Required",
            /* 403 */ "Forbidden",
            /* 404 */ "Not Found",
            /* 405 */ "Method Not Allowed",
            /* 406 */ "Not Acceptable",
            /* 407 */ "Proxy Authentication Required",
            /* 408 */ "Request Timeout",
            /* 409 */ "Conflict",
            /* 410 */ "Gone",
            /* 411 */ "Length Required",
            /* 412 */ "Precondition Failed",
            /* 413 */ "Request Entity Too Large",
            /* 414 */ "Request-Uri Too Long",
            /* 415 */ "Unsupported Media Type",
            /* 416 */ "Requested Range Not Satisfiable",
            /* 417 */ "Expectation Failed",
            /* 418 */ String.Empty,
            /* 419 */ String.Empty,
            /* 420 */ String.Empty,
            /* 421 */ String.Empty,
            /* 422 */ "Unprocessable Entity",
            /* 423 */ "Locked",
            /* 424 */ "Failed Dependency"
        },
    
        new String[]
        { 
            /* 500 */"Internal Server Error",
            /* 501 */ "Not Implemented",
            /* 502 */ "Bad Gateway",
            /* 503 */ "Service Unavailable",
            /* 504 */ "Gateway Timeout",
            /* 505 */ "Http Version Not Supported",
            /* 506 */ String.Empty,
            /* 507 */ "Insufficient Storage"
        }
    };
    
    public static String GetStatusDescription(int code) {
        if (code >= 100 && code < 600) {
            int i = code / 100;
            int j = code % 100;
 
            if (j < s_HTTPStatusDescriptions[i].Length)
                return s_HTTPStatusDescriptions[i][j];
        }
 
        return String.Empty;
    }
}