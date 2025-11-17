using System.Data;

namespace Storm.Api.Databases.Configurations;

public delegate void DatabaseInterceptorDelegate(IDbCommand command, object item);