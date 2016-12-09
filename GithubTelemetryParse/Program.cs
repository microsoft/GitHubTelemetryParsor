using System;
using System.Collections.Generic;
using GithubTelemetryParse.Models;
using RestSharp;
using RestSharp.Authenticators;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using Path = GithubTelemetryParse.Models.Path;
using System.Configuration;

namespace GithubTelemetryParse
{
    class Program
    {
        static void Main()
        {
            //Get Data from Appsettings
            var gitHubAccount = ConfigurationManager.AppSettings["GitHubAccount"];
            var gitHubPersonalToken = ConfigurationManager.AppSettings["GitHubPersonalToken"];
            var sqlConnectionString = ConfigurationManager.AppSettings["SQLConnectionString"];
            var reposPath = ConfigurationManager.AppSettings["ReposPath"];    
            
            //Github client
            var client = new RestClient("https://api.github.com/repos/" + reposPath );
            client.Authenticator = new HttpBasicAuthenticator(gitHubAccount, gitHubPersonalToken);

            //Get Views Data
            var viewRequest = new RestRequest("traffic/views", Method.GET);
            viewRequest.AddHeader("Accept", "application/vnd.github.spiderman-preview");
            IRestResponse viewResponse = client.Execute(viewRequest);
            var viewJson = viewResponse.Content;
            var views = JsonConvert.DeserializeObject<Views>(viewJson);

            //Get Downloads Data
            var downloadRequest = new RestRequest("releases", Method.GET);
            IRestResponse downloadResponse = client.Execute(downloadRequest);
            var downloadJson = downloadResponse.Content;
            var downloads = JsonConvert.DeserializeObject<IEnumerable<Release>>(downloadJson);

            //Get Popular Path Data
            var pathRequest = new RestRequest("traffic/popular/paths", Method.GET);
            pathRequest.AddHeader("Accept", "application/vnd.github.spiderman-preview");
            IRestResponse pathResponse = client.Execute(pathRequest);
            var pathJson = pathResponse.Content;
            var paths = JsonConvert.DeserializeObject<IEnumerable<Path>>(pathJson);

            //Get Top 10 Referrer Data
            var referrerRequest = new RestRequest("traffic/popular/referrers", Method.GET);
            referrerRequest.AddHeader("Accept", "application/vnd.github.spiderman-preview");
            IRestResponse referrerResponse = client.Execute(referrerRequest);
            var referrerJson = referrerResponse.Content;
            var referrers = JsonConvert.DeserializeObject<IEnumerable<Referrer>>(referrerJson);


            //Write the data to SQL
            using (
                var connection = new SqlConnection(sqlConnectionString)
            )
            {
                connection.Open();

                InsertOrUpdateViewsTwoWeeks(connection, views);
                InsertOrUpdateViewsDate(connection, views);
                InsertOrUpdateDownloadsDate(connection, downloads);
                InsertOrUpdatePathTwoWeeks(connection, paths);
                InsertOrUpdateReferrerTwoWeeks(connection, referrers);

                connection.Close();
            }

        }

        public static void InsertOrUpdateViewsTwoWeeks(SqlConnection connection, Views views)
        {

            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = @"
                if exists (select * from GitHub.ViewsByTwoWeeks where Date = @date)
                begin
                update GitHub.ViewsByTwoWeeks
                set count = @count, uniques = @uniques, updated_time = @updated_time
                where Date = @date
                end
                else
                begin
                insert into GitHub.ViewsByTwoWeeks (Date, count, uniques, updated_time)
                values (@date, @count, @uniques, @updated_time)
                end
                ";

                command.Parameters.AddWithValue("@date", DateTime.UtcNow.Date);
                command.Parameters.AddWithValue("@count", views.count);
                command.Parameters.AddWithValue("@uniques", views.uniques);
                command.Parameters.AddWithValue("@updated_time", DateTime.UtcNow);

                command.ExecuteNonQuery();
            }
        }

        public static void InsertOrUpdateViewsDate(SqlConnection connection, Views views)
        {


            foreach (var view in views.views)
            {
                if (!(view.timestamp.Date == DateTime.UtcNow.Date))
                {
                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = @"
                        if not exists (select * from GitHub.ViewsByDate where Date = @date)
                        begin
                        insert into GitHub.ViewsByDate (Date, count, uniques, updated_time)
                        values (@date, @count, @uniques, @updated_time)
                        end
                        ";

                        command.Parameters.AddWithValue("@date", view.timestamp.Date);
                        command.Parameters.AddWithValue("@count", view.count);
                        command.Parameters.AddWithValue("@uniques", view.uniques);
                        command.Parameters.AddWithValue("@updated_time", DateTime.UtcNow);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void InsertOrUpdateDownloadsDate(SqlConnection connection, IEnumerable<Release> releases )
        {

            foreach (var release in releases)
            {
                foreach (var asset in release.assets)
                {
                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = @"
                        if exists (select * from GitHub.DownloadsByDate where download_id = @download_id)
                        begin
                        update GitHub.DownloadsByDate
                        set updated_time = @updated_time, version = @version, download_count = @download_count, name = @name
                        where download_id = @download_id
                        end
                        else
                        begin
                        insert into GitHub.DownloadsByDate (updated_time, version, download_count, name, download_id)
                        values (@updated_time, @version, @download_count, @name, @download_id)
                        end
                        ";

                        command.Parameters.AddWithValue("@version", release.tag_name);
                        command.Parameters.AddWithValue("@download_count", asset.download_count);
                        command.Parameters.AddWithValue("@name", asset.name);
                        command.Parameters.AddWithValue("@download_id", asset.id + DateTime.UtcNow.ToString("yyyyMMdd"));
                        command.Parameters.AddWithValue("@updated_time", DateTime.UtcNow);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void InsertOrUpdatePathTwoWeeks(SqlConnection connection, IEnumerable<Path> paths )
        {
            int order = 0;

            foreach (var path in paths)
            {
                order++;

                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"
                    if exists (select * from GitHub.PathsByTwoWeeks where path_id = @path_id)
                    begin
                    update GitHub.PathsByTwoWeeks
                    set updated_time = @updated_time, count = @count, uniques = @uniques, path = @path
                    where path_id = @path_id
                    end
                    else
                    begin
                    insert into GitHub.PathsByTwoWeeks (updated_time, count, uniques, path, path_id)
                    values (@updated_time, @count, @uniques, @path, @path_id)
                    end
                    ";

                    command.Parameters.AddWithValue("@count", path.count);
                    command.Parameters.AddWithValue("@uniques", path.uniques);
                    command.Parameters.AddWithValue("@path", path.path);
                    command.Parameters.AddWithValue("@path_id",
                        DateTime.UtcNow.Date.ToString("yyyyMMdd") + order.ToString("D2"));
                    command.Parameters.AddWithValue("@updated_time", DateTime.UtcNow);

                    command.ExecuteNonQuery();
                }
            }
        }

        public static void InsertOrUpdateReferrerTwoWeeks(SqlConnection connection, IEnumerable<Referrer> referrers)
        {
            int order = 0;

            foreach (var referrer in referrers)
            {
                order++;

                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"
                    if exists (select * from GitHub.ReferrersByTwoWeeks where referrer_id = @referrer_id)
                    begin
                    update GitHub.ReferrersByTwoWeeks
                    set updated_time = @updated_time, count = @count, uniques = @uniques, referrer = @referrer
                    where referrer_id = @referrer_id
                    end
                    else
                    begin
                    insert into GitHub.ReferrersByTwoWeeks (updated_time, count, uniques, referrer, referrer_id)
                    values (@updated_time, @count, @uniques, @referrer, @referrer_id)
                    end
                    ";

                    command.Parameters.AddWithValue("@date", DateTime.UtcNow.Date);
                    command.Parameters.AddWithValue("@count", referrer.count);
                    command.Parameters.AddWithValue("@uniques", referrer.uniques);
                    command.Parameters.AddWithValue("@referrer", referrer.referrer);
                    command.Parameters.AddWithValue("@referrer_id",
                        DateTime.UtcNow.Date.ToString("yyyyMMdd") + order.ToString("D2"));
                    command.Parameters.AddWithValue("@updated_time", DateTime.UtcNow);

                    command.ExecuteNonQuery();
                }
            }
        }


    }
}
