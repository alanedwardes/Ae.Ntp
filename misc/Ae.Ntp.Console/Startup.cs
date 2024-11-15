namespace Ae.Ntp.Console
{
    public sealed class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
            });
        }
    }
}