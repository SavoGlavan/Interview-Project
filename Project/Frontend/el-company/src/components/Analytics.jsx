import React, { useEffect, useState } from "react";
import { Card, Row, Col, Spin, Typography, message } from "antd";
import { PieChart, Pie, Cell, Tooltip, Legend, BarChart, Bar, XAxis, YAxis, ResponsiveContainer } from "recharts";
import { userService } from "../services/UserService";

const { Title } = Typography;

// Define chart colors
const COLORS = ["#fa8c16", "#13c2c2", "#52c41a", "#722ed1", "#eb2f96", "#1890ff"];

const Analytics = () => {
  const [planData, setPlanData] = useState([]);
  const [taxGroupData, setTaxGroupData] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchAnalytics = async () => {
      try {
        const [plansResponse, taxGroupsResponse] = await Promise.all([
          userService.getUserCountByPlan(),
          userService.getUserCountByTaxGroup(),
        ]);

        // Assuming API returns array like: [{ name: "Standard", count: 12 }, { name: "Premium", count: 8 }]
        setPlanData(plansResponse);
        setTaxGroupData(taxGroupsResponse);
      } catch (err) {
        console.error(err);
        message.error("Failed to load analytics data.");
      } finally {
        setLoading(false);
      }
    };

    fetchAnalytics();
  }, []);

  if (loading) {
    return (
      <div style={{ textAlign: "center", padding: 60 }}>
        <Spin size="large" />
      </div>
    );
  }

  return (
    <div style={{ padding: 24 }}>
      <Title level={2} style={{ color: "#fa8c16", marginBottom: 32 }}>
        User Analytics
      </Title>

      <Row gutter={[24, 24]}>
        {/* Plan Popularity */}
        <Col xs={24} lg={12}>
          <Card
            title="Plan Popularity"
            style={{ borderRadius: 12, boxShadow: "0 4px 12px rgba(0,0,0,0.05)" }}
          >
            {planData.length === 0 ? (
              <p>No data available</p>
            ) : (
            <ResponsiveContainer width="100%" height={300}>
                <BarChart data={planData}>
                    <XAxis dataKey="plan" />
                    <YAxis />
                    <Tooltip />
                    <Legend />
                    <Bar dataKey="userCount" radius={[6, 6, 0, 0]} name="Number of users">
                    {planData.map((entry, index) => (
                        <Cell
                        key={`cell-${index}`}
                        fill={
                            [
                            "#fa8c16", // orange
                            "#1890ff", // blue
                            "#52c41a", // green
                            "#eb2f96", // pink
                            "#722ed1", // purple
                            "#13c2c2", // cyan
                            ][index % 6] // cycle colors
                        }
                        />
                    ))}
                    </Bar>
                </BarChart>
            </ResponsiveContainer>

            )}
          </Card>
        </Col>

        {/* Tax Group Distribution */}
        <Col xs={24} lg={12}>
          <Card
            title="Tax Group Distribution"
            style={{ borderRadius: 12, boxShadow: "0 4px 12px rgba(0,0,0,0.05)" }}
          >
            {taxGroupData.length === 0 ? (
              <p>No data available</p>
            ) : (
              <ResponsiveContainer width="100%" height={300}>
                <PieChart>
                  <Pie
                    data={taxGroupData}
                    dataKey="userCount"
                    nameKey="taxGroup"
                    cx="50%"
                    cy="50%"
                    outerRadius={100}
                    label
                  >
                    {taxGroupData.map((_, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip />
                  <Legend />
                </PieChart>
              </ResponsiveContainer>
            )}
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default Analytics;
