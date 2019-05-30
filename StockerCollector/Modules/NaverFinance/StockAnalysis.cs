using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Connector;
using Model.Common;
using Model.Request;

namespace Finance
{
    public class StockAnalysis
    {
        private StockAnalysis()
        {
        }

        public static StockAnalysis Instance
        {
            get
            {
                return Nested<StockAnalysis>.Instance;
            }
        }

        public enum TrendType
        {
            Upward,
            Downward
        }

        public List<JsonDictionary> AllAnalysis(SetDataSourceReq data_source)
        {
            var key = "종가";
            var time_key = "날짜";
            var result = new List<JsonDictionary>();
            for (var i = 1; i <= data_source.rawdata.Count; i++)
            {
                var origin = new List<JsonDictionary>(data_source.rawdata.GetRange(0, i));
                var ret_data = SingleAnalysis(origin, key, i, time_key);
                var test = new JsonDictionary(ret_data.GetDictionary());
                result.Add(test);
            }
            return result;
        }

        private JsonDictionary SingleAnalysis(List<JsonDictionary> data_source, string key, int i, string time_key)
        {
            var test = new Dictionary<string, List<string>>();
            test.Add("support", new List<string>());
            test.Add("resistance", new List<string>());
            test.Add("result", new List<string>());
            var analysis_data = data_source;

            var max = analysis_data.Aggregate<JsonDictionary>((arg1, arg2) =>
            {
                return double.Parse(arg1[key].ToString()) > double.Parse(arg2[key].ToString()) ? arg1 : arg2;
            });

            var min = analysis_data.Aggregate<JsonDictionary>((arg1, arg2) =>
            {
                return double.Parse(arg1[key].ToString()) < double.Parse(arg2[key].ToString()) ? arg1 : arg2;
            });

            Segmentation(ref test, ref analysis_data, analysis_data, key, double.Parse(max[key].ToString()), double.Parse(min[key].ToString()), time_key);

            var datum = analysis_data[i - 1];

            int calc_cnt = 0;
            double life_price = 0;

            for (int m = 0; m < 60; m++)
            {
                var index = analysis_data.Count - m - 1;
                if (index < 0)
                {
                    break;
                }
                else
                {
                    if (calc_cnt == 20) data_source[i - 1].Add("20평균가", life_price / calc_cnt);
                    life_price += double.Parse(analysis_data[index][key].ToString());
                }
                calc_cnt++;
            }
            data_source[i - 1].Add("60평균가", life_price / calc_cnt);

            var prevCount = 0;
            var currentCount = 0;
            var lastState = "횡보";
            var supportArr = new JsonArray();
            var resistanceArr = new JsonArray();
            var total_resists = 0;
            var total_supports = 0;
            var last_support = 0.0;
            var last_resist = 0.0;
            foreach (var data_key in datum.GetDictionary().Keys)
            {
                if (data_key.Contains("support"))
                {
                    if (lastState == "하락")
                    {
                        prevCount = currentCount;
                        currentCount = 0;
                    }
                    lastState = "상승";
                    supportArr.Add(double.Parse(datum[data_key].ToString()));
                    last_support = double.Parse(datum[data_key].ToString());
                    currentCount++;
                    total_supports++;
                }
                else if (data_key.Contains("resistance"))
                {
                    if (lastState == "상승")
                    {
                        prevCount = currentCount;
                        currentCount = 0;
                    }
                    lastState = "하락";
                    resistanceArr.Add(double.Parse(datum[data_key].ToString()));
                    last_resist = double.Parse(datum[data_key].ToString());
                    currentCount++;
                    total_resists++;
                }
            }
            data_source[i - 1].Add("last_resist", last_resist);
            data_source[i - 1].Add("last_support", last_support);
            data_source[i - 1].Add("resists", total_resists);
            data_source[i - 1].Add("supports", total_supports);
            double price = double.Parse(datum[key].ToString());
            double start_price = double.Parse(datum["시가"].ToString());
            var real_support = supportArr.Where<JsonValue>(p => p.ReadAs<double>() <= price);
            var reverse_support = supportArr.Where<JsonValue>(p => p.ReadAs<double>() >= price);
            var real_resistance = resistanceArr.Where<JsonValue>(p => p.ReadAs<double>() >= price);
            var reverse_resistance = resistanceArr.Where<JsonValue>(p => p.ReadAs<double>() <= price);

            var total_support = new JsonArray();
            var total_resistance = new JsonArray();
            total_support.AddRange(real_support);
            total_support.AddRange(reverse_resistance);
            total_resistance.AddRange(real_resistance);
            total_resistance.AddRange(reverse_support);
            var v_pattern_real = double.Parse(real_support.Count().ToString()) /
                                (double.Parse(reverse_support.Count().ToString()) + double.Parse(real_support.Count().ToString())) * 100;
            var v_pattern_reverse = double.Parse(reverse_resistance.Count().ToString()) /
                                    (double.Parse(real_resistance.Count().ToString()) + double.Parse(reverse_resistance.Count().ToString())) * 100;
            var v_pattern = ((double.IsNaN(v_pattern_real) || double.IsInfinity(v_pattern_real) ? 0 : v_pattern_real) +
                            (double.IsNaN(v_pattern_reverse) || double.IsInfinity(v_pattern_reverse) ? 0 : v_pattern_reverse));

            var a_pattern_real = double.Parse(real_resistance.Count().ToString()) /
                                (double.Parse(reverse_resistance.Count().ToString()) + double.Parse(real_resistance.Count().ToString())) * 100;
            var a_pattern_reverse = double.Parse(reverse_support.Count().ToString()) /
                                    (double.Parse(real_support.Count().ToString()) + double.Parse(reverse_support.Count().ToString())) * 100;
            var a_pattern = ((double.IsNaN(a_pattern_real) || double.IsInfinity(a_pattern_real) ? 0 : a_pattern_real) +
                            (double.IsNaN(a_pattern_reverse) || double.IsInfinity(a_pattern_reverse) ? 0 : a_pattern_reverse));

            var totalState = string.Empty;
            if (v_pattern > a_pattern)
            {
                // 상승을 하였으며, A패턴 비율에 따라 조정강도 파악 가능 (A패턴_비율로 오름차순정렬)
                totalState = "상승";
            }
            else if (v_pattern < a_pattern)
            {
                // 하락을 하였으며, V패턴 비율에 따라 반등강도 파악 가능 (V패턴_비율로 오름차순정렬)
                totalState = "하락";
            }
            else
            {
                if (total_support.Count > total_resistance.Count)
                {
                    totalState = "상승";
                }
                else if (total_support.Count < total_resistance.Count)
                {
                    totalState = "하락";
                }
                else
                {
                    totalState = "횡보";
                }
            }
            if (start_price == 0)
            {
                lastState = "거래정지";
                totalState = "거래정지";
            }

            data_source[i - 1].Add("현재상태", lastState);
            data_source[i - 1].Add("전체상태", totalState);
            data_source[i - 1].Add("저항갯수", real_resistance.Count() + reverse_support.Count());
            data_source[i - 1].Add("지지갯수", real_support.Count() + reverse_resistance.Count());
            data_source[i - 1].Add("최근갯수", currentCount);
            data_source[i - 1].Add("과거갯수", prevCount);
            data_source[i - 1].Add("V패턴_비율", v_pattern);
            data_source[i - 1].Add("A패턴_비율", a_pattern);
            data_source[i - 1].Add("강도", v_pattern - a_pattern);

            if (test["result"].Count > 0)
            {
                var supstance = "";
                foreach (var support_price in test["result"])
                {
                    supstance += support_price + ",";
                }
                supstance = supstance.Substring(0, supstance.Length - 1);
                data_source[i - 1].Add("지지가격대", supstance);
            }
            return data_source[i - 1];
        }

        public JsonDictionary AutoAnalysis(string collectionName, string code, long siseUnix, JsonDictionary data_source)
        {
            var source = "stock";
            var field = "종가";
            var sampling = "min";
            var sampling_period = "day";

            var category = code;
            var queryBuilder = new StringBuilder();
            var sampling_items = new StringBuilder();
            queryBuilder.Append("SELECT {sampling_items} UNIX_TIMESTAMP(DATE(unixtime)) as `날짜` FROM (SELECT ");

            var item_key = field;
            queryBuilder.Append("COLUMN_GET(`rawdata`,'").Append(item_key).Append("' as double) as `").Append(item_key).Append("`,");
            sampling_items.Append(sampling).Append("(`").Append(item_key).Append("`) as `").Append(item_key).Append("`,");
            queryBuilder.Append("unixtime ").Append("FROM ").Append("past_" + source).Append(" WHERE category = '")
                .Append(category).Append("' AND column_get(rawdata,'").Append(item_key).Append("' as char) IS NOT NULL");
            queryBuilder.Append(" AND unixtime < FROM_UNIXTIME(").Append(siseUnix).Append(")) as result");


            if (sampling_period == "all") queryBuilder.Append(" GROUP BY unixtime ASC");
            else if (sampling_period == "day") queryBuilder.Append(" GROUP BY DATE(unixtime) ASC");
            else if (sampling_period == "week") queryBuilder.Append(" GROUP BY TO_DAYS(unixtime) - WEEKDAY(unixtime) ASC");
            else if (sampling_period == "month") queryBuilder.Append(" GROUP BY DATE_FORMAT(unixtime, '%Y-%m') ASC");
            else if (sampling_period == "year") queryBuilder.Append(" GROUP BY DATE_FORMAT(unixtime, '%Y') ASC");

            var query = queryBuilder.ToString().Replace("{sampling_items}", sampling_items.ToString());

            var wow = MariaDBConnector.Instance.GetQuery("DynamicQueryExecuter", query);

            wow.Add(data_source);

            return this.SingleAnalysis(wow, field, wow.Count, "날짜");
        }

        private void Segmentation(ref Dictionary<string, List<string>> test, ref List<JsonDictionary> result, List<JsonDictionary> data,
            string key, double maximum, double minimum, string time_key, double? startUnixTime = null)
        {
            if (startUnixTime != null)
            {
                data = data.Where<JsonDictionary>(p => double.Parse(p[time_key].ToString()) >= startUnixTime).ToList<JsonDictionary>();
                if (data == null || data.Count == 1)
                {
                    return;
                }
            }

            var max = data.Aggregate<JsonDictionary>((arg1, arg2) =>
            {
                return double.Parse(arg1[key].ToString()) > double.Parse(arg2[key].ToString()) ? arg1 : arg2;
            });
            var min = data.Aggregate<JsonDictionary>((arg1, arg2) =>
            {
                return double.Parse(arg1[key].ToString()) < double.Parse(arg2[key].ToString()) ? arg1 : arg2;
            });

            var trendType = double.Parse(max[time_key].ToString()) > double.Parse(min[time_key].ToString()) ? TrendType.Upward : TrendType.Downward;
            var result2 = new List<JsonDictionary>();
            var lastIndex = result.Count;

            var up_count = 0;
            double total_value = 0;
            switch (trendType)
            {
                case TrendType.Upward:
                    {
                        var internalData = data.Where<JsonDictionary>((p) =>
                        {
                            return double.Parse(p[time_key].ToString()) > double.Parse(min[time_key].ToString())
                            && double.Parse(p[time_key].ToString()) < double.Parse(max[time_key].ToString());
                        }).ToList<JsonDictionary>();
                        this.TrendAnalysis(key, min, max, internalData, ref result2, 0, double.Parse(min[key].ToString()), time_key);

                        foreach (var inc in result2)
                        {
                            double prev_value = 0;
                            var unixtime = int.Parse(inc[time_key].ToString());

                            var id = key + "_support_" + EnvironmentHelper.GetDateTimeString(unixtime);
                            test["support"].Add(id);
                            var next = 0;
                            var complete = false;
                            for (int i = result.IndexOf(min); i < lastIndex; i++)
                            {
                                if (complete) break;
                                var dynamicUnixtime = double.Parse(result[i][time_key].ToString());
                                var diff = double.Parse(inc["diff"].ToString());
                                var nextValue = double.Parse(min[key].ToString()) + (diff * next);
                                if (!(nextValue <= maximum))
                                {
                                    result[i].Add(id, nextValue);
                                    complete = true;
                                }
                                else if (i == lastIndex - 1)
                                {
                                    result[i].Add(id, nextValue);
                                    complete = true;
                                }
                                else
                                {
                                    if (test["resistance"].Count > 0)
                                    {
                                        foreach (var item in test["resistance"])
                                        {
                                            if (result[i].ContainsKey(item))
                                            {
                                                var resist_value = double.Parse(result[i][item].ToString());
                                                if (nextValue >= resist_value && prev_value < resist_value && prev_value > 0)
                                                {
                                                    total_value += resist_value;
                                                    up_count++;
                                                }
                                            }
                                        }
                                    }
                                    result[i].Add(id, nextValue);
                                    prev_value = nextValue;
                                }
                                next++;
                            }
                        }
                        if (up_count > 0)
                        {
                            var support_price = Math.Truncate(total_value / up_count / 10) * 10;
                            if (!test["result"].Contains(support_price.ToString()))
                            {
                                test["result"].Add(support_price.ToString());
                            }
                        }

                        this.Segmentation(ref test, ref result, data, key, maximum, minimum, time_key, double.Parse(max[time_key].ToString()));
                        break;
                    }
                case TrendType.Downward:
                    {
                        var internalData = data.Where<JsonDictionary>((p) =>
                        {
                            return double.Parse(p[time_key].ToString()) < double.Parse(min[time_key].ToString())
                            && double.Parse(p[time_key].ToString()) > double.Parse(max[time_key].ToString());
                        }).ToList<JsonDictionary>();
                        this.TrendAnalysis(key, max, min, internalData, ref result2, 0, double.Parse(max[key].ToString()), time_key);

                        foreach (var dec in result2)
                        {
                            double prev_value = 0;
                            var unixtime = int.Parse(dec[time_key].ToString());
                            var next = 0;
                            var id = key + "_resistance_" + EnvironmentHelper.GetDateTimeString(unixtime);
                            test["resistance"].Add(id);
                            var complete = false;
                            for (int i = result.IndexOf(max); i < lastIndex; i++)
                            {
                                if (complete) break;
                                var dynamicUnixtime = double.Parse(result[i][time_key].ToString());

                                var diff = double.Parse(dec["diff"].ToString());
                                var nextValue = double.Parse(max[key].ToString()) + (diff * next);
                                if (!(nextValue >= minimum))
                                {
                                    result[i].Add(id, nextValue);
                                    complete = true;
                                }
                                else if (i == lastIndex - 1)
                                {
                                    result[i].Add(id, nextValue);
                                    complete = true;
                                }
                                else
                                {
                                    if (test["support"].Count > 0)
                                    {
                                        foreach (var item in test["support"])
                                        {
                                            if (result[i].ContainsKey(item))
                                            {
                                                var support_value = double.Parse(result[i][item].ToString());
                                                if (nextValue <= support_value && prev_value > support_value && prev_value > 0)
                                                {
                                                    total_value += support_value;
                                                    up_count++;
                                                }
                                            }
                                        }
                                    }
                                    result[i].Add(id, nextValue);
                                    prev_value = nextValue;
                                }
                                next++;
                            }
                        }

                        if (up_count > 0)
                        {
                            var resist_price = Math.Truncate(total_value / up_count / 10) * 10;
                            if (!test["result"].Contains(resist_price.ToString()))
                            {
                                test["result"].Add(resist_price.ToString());
                            }
                        }

                        this.Segmentation(ref test, ref result, data, key, maximum, minimum, time_key, double.Parse(min[time_key].ToString()));
                        break;
                    }
            }
        }

        private void TrendAnalysis(string key, JsonDictionary start, JsonDictionary end, List<JsonDictionary> data, ref List<JsonDictionary> result, int prevIndex, double firstValue, string time_key)
        {
            if (data == null || data.Count() == 0) return;

            var startX = double.Parse(start[time_key].ToString());
            var startY = double.Parse(start[key].ToString());
            var endX = double.Parse(end[time_key].ToString());
            var endY = double.Parse(end[key].ToString());
            var standardDegree = Math.Atan2(Math.Abs(endY - startY), (Math.Abs(endX - startX))) * 180d / Math.PI;

            var index = prevIndex;
            JsonDictionary minimum = null;
            double? prevDegree = null;
            foreach (var item in data)
            {
                var dynamicX = double.Parse(item[time_key].ToString());
                var dynamicY = double.Parse(item[key].ToString());
                var dynamicDegree = Math.Atan2(Math.Abs(dynamicY - startY), (Math.Abs(dynamicX - startX))) * 180d / Math.PI;
                if (prevDegree != null)
                {
                    index++;
                    if (prevDegree > dynamicDegree && standardDegree > dynamicDegree)
                    {
                        minimum[key] = dynamicY;
                        minimum[time_key] = dynamicX;
                        minimum["degree"] = dynamicDegree;
                        minimum["index"] = index;
                        minimum["diff"] = (dynamicY - firstValue) / index;

                        prevDegree = dynamicDegree;
                    }
                }
                if (index == prevIndex)
                {
                    index++;
                    prevDegree = dynamicDegree;
                    minimum = new JsonDictionary();
                    minimum.Add(key, dynamicY);
                    minimum.Add(time_key, dynamicX);
                    minimum.Add("degree", dynamicDegree);
                    minimum.Add("index", index);
                    minimum.Add("diff", (dynamicY - firstValue) / index);
                }
            }

            if (minimum == null) return;

            result.Add(minimum);
            index = int.Parse(minimum["index"].ToString());
            var next_data = data.Where(p => double.Parse(p[time_key].ToString()) > double.Parse(minimum[time_key].ToString())).ToList<JsonDictionary>();
            this.TrendAnalysis(key, minimum, end, next_data, ref result, index, firstValue, time_key);
        }

        internal void AutoAnalysis(string p, ref List<JsonDictionary> list)
        {
            throw new NotImplementedException();
        }

        public void AutoAnalysis(string p, ref SetDataSourceReq result)
        {
            throw new NotImplementedException();
        }
    }
}
