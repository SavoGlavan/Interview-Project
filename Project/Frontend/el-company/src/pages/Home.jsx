import React, { useEffect, useState } from "react";
import {
  Card,
  Button,
  Modal,
  InputNumber,
  Select,
  Spin,
  Typography,
  Space,
  Tag,
  message,
  Divider,
} from "antd";
import { planService } from "../services/PlanService";
import { taxGroupService } from "../services/TaxGroupService";
import { userService } from "../services/UserService";
import { authService } from "../services/AuthService";

const { Title, Text } = Typography;
const { Option } = Select;

const Home = () => {
  const [plans, setPlans] = useState([]);
  const [taxGroups, setTaxGroups] = useState([]);
  const [loading, setLoading] = useState(true);

  // PLAN SELECTION MODAL STATE
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [selectedPlan, setSelectedPlan] = useState(null);
  const [selectedTaxGroup, setSelectedTaxGroup] = useState(null);
  const [consumption, setConsumption] = useState(0);
  const [calculatedCost, setCalculatedCost] = useState(null);
  const [confirming, setConfirming] = useState(false);

  // HIGHLIGHT CURRENTLY SELECTED PLAN
  const [userSelectedPlanId, setUserSelectedPlanId] = useState(null);

  // RECOMMENDATION STATE
  const [recommendedPlan, setRecommendedPlan] = useState(null);
  const [isRecommendationModalVisible, setIsRecommendationModalVisible] =
    useState(false);
  const [recConsumption, setRecConsumption] = useState(0);
  const [recTaxGroup, setRecTaxGroup] = useState(null);
  const [loadingRecommendation, setLoadingRecommendation] = useState(false);

  const user = authService.getUserFromToken();

  // Load all data + user's saved plan
  useEffect(() => {
    const fetchData = async () => {
      try {
        const [plansData, taxGroupsData] = await Promise.all([
          planService.getAll(),
          taxGroupService.getAll(),
        ]);

        setPlans(plansData);
        setTaxGroups(taxGroupsData);

        // Fetch user selected plan
        if (user) {
          const userData = await userService.getById(user.id);
          if (userData?.details.planId) {
            setUserSelectedPlanId(userData.details.planId);
          }
        }
      } catch (err) {
        console.error(err);
        message.error("Failed to load data.");
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  // --- SELECT PLAN LOGIC ---
  const openModal = (plan) => {
    setSelectedPlan(plan);
    setSelectedTaxGroup(null);
    setConsumption(0);
    setCalculatedCost(null);
    setIsModalVisible(true);
  };

  const closeModal = () => {
    setIsModalVisible(false);
    setSelectedPlan(null);
    setSelectedTaxGroup(null);
    setConsumption(0);
    setCalculatedCost(null);
  };

  const calculateCost = () => {
    if (!selectedPlan || !selectedTaxGroup || consumption <= 0) {
      message.warning("Please select a tax group and enter consumption.");
      return;
    }

    let remaining = consumption;
    let baseCost = 0;
    let previousThreshold = 0;

    const tiers = [...selectedPlan.prices].sort(
      (a, b) => (a.threshold ?? Infinity) - (b.threshold ?? Infinity)
    );

    for (let tier of tiers) {
      const tierThreshold = tier.threshold ?? Infinity;
      const kwhInTier = Math.min(remaining, tierThreshold - previousThreshold);

      if (kwhInTier > 0) {
        baseCost += kwhInTier * tier.price;
        remaining -= kwhInTier;
        previousThreshold = tierThreshold;
      }
    }

    if (remaining > 0) {
      const lastTierPrice = tiers[tiers.length - 1]?.price ?? 0;
      baseCost += remaining * lastTierPrice;
    }

    const discountAmount = selectedPlan.discount
      ? (baseCost * selectedPlan.discount) / 100
      : 0;

    const costAfterDiscount = baseCost - discountAmount;

    const vatAmount =
      ((selectedTaxGroup.vat ?? 0) / 100) * costAfterDiscount;

    const ecoAmount =
      ((selectedTaxGroup.ecoTax ?? 0) / 100) * costAfterDiscount;

    const total = costAfterDiscount + vatAmount + ecoAmount;

    setCalculatedCost({
      baseCost,
      discountAmount,
      costAfterDiscount,
      vatAmount,
      ecoAmount,
      total,
    });
  };

  const confirmPlanSelection = async () => {
    if (!selectedPlan || !selectedTaxGroup || !calculatedCost) {
      message.warning("Please calculate your cost before confirming.");
      return;
    }

    if (!user) {
      message.error("User not logged in.");
      return;
    }

    setConfirming(true);

    try {
      await userService.setUserPlan(
        user.id,
        selectedTaxGroup.id,
        selectedPlan.id,
        consumption
      );

      setUserSelectedPlanId(selectedPlan.id);
      message.success("Plan successfully selected!");
      closeModal();
    } catch (err) {
      console.error(err);
      message.error("Failed to set your plan. Please try again.");
    } finally {
      setConfirming(false);
    }
  };

  // --- RECOMMENDATION LOGIC ---
  const getRecommendation = async () => {
    if (!recConsumption || !recTaxGroup) {
      message.warning("Please enter consumption & select tax group.");
      return;
    }

    setLoadingRecommendation(true);

    try {
      const result = await planService.getRecommendation(
        recConsumption,
        recTaxGroup.id
      );

      setRecommendedPlan(result.planDTO);
      message.success("Recommended plan loaded!");
      setIsRecommendationModalVisible(false);
    } catch (err) {
      console.error(err);
      message.error("Failed to load recommendation.");
    } finally {
      setLoadingRecommendation(false);
    }
  };

  // --- RENDER ---

  if (loading) {
    return (
      <div style={{ textAlign: "center", padding: 50 }}>
        <Spin size="large" />
      </div>
    );
  }

  return (
    <div style={{ padding: 24, minHeight: "100vh" }}>
      <Title level={2} style={{ color: "#fa8c16", marginBottom: 24 }}>
        Available Plans
      </Title>

     

      {/* ALL PLANS */}
      <Space wrap size="middle">
        {plans.map((plan) => (
          <Card
            key={plan.id}
            title={plan.name}
            style={{
              width: 300,
              borderRadius: 16,
              minHeight: "35vh",
              boxShadow: "0 4px 12px rgba(0,0,0,0.1)",
              transition: "0.3s",
              border:
                plan.id === userSelectedPlanId
                  ? "3px solid #fa8c16"
                  : "1px solid #d9d9d9",
              background:
                plan.id === userSelectedPlanId ? "#fff7e6" : "white",
            }}
            extra={
              plan.discount ? (
                <Tag color="green">{plan.discount}% off</Tag>
              ) : null
            }
          >
            <div style={{ marginBottom: 12 }}>
              {plan.prices?.length > 0 ? (
                plan.prices.map((p, idx) => (
                  <div key={idx}>
                    <Text>
                      {p.threshold ?? "Remaining"} kWh → {p.price} €/kWh
                    </Text>
                  </div>
                ))
              ) : (
                <Text>—</Text>
              )}
            </div>

            <Button
              type="primary"
              block
              style={{ marginTop: 12 }}
              onClick={() => openModal(plan)}
            >
              Choose This Plan
            </Button>
          </Card>
        ))}
      </Space>
       {/* RECOMMENDATION SECTION */}
      <div
        style={{
          display: "flex",
          alignItems: "center",
          gap: 16,
          marginBottom: 24,
        }}
      >
        <Text strong style={{ fontSize: 18, padding:20}}>
          Unsure which plan is best?
        </Text>

        <Button
          type="primary"
          size="middle"
          style={{
            borderRadius: 8,
            padding: "0 22px",
            height: 40,
            fontSize: 16,
            background: "#52c41a",
            borderColor: "#52c41a",
          }}
          onClick={() => setIsRecommendationModalVisible(true)}
        >
          Recommend Plan
        </Button>
      </div>

      {/* Recommended Plan Card */}
      {recommendedPlan && (
        <Card
          style={{
            marginBottom: 32,
            borderRadius: 16,
            border: "3px solid #52c41a",
            background: "#f6ffed",
            boxShadow: "0 4px 12px rgba(0,0,0,0.1)",
            width:"250px"
          }}
        >
          <Title level={3} style={{ color: "#389e0d" }}>
            Recommended Plan: {recommendedPlan.name}
          </Title>

          {recommendedPlan.prices?.map((p, idx) => (
            <Text key={idx} style={{ display: "block" }}>
              {p.threshold ?? "Remaining"} kWh → {p.price} €/kWh
            </Text>
          ))}

          {recommendedPlan.discount ? (
            <Tag color="green" style={{ marginTop: 8 }}>
              {recommendedPlan.discount}% off
            </Tag>
          ) : null}
        </Card>
      )}

      {/* SELECT PLAN MODAL */}
      <Modal
        title={selectedPlan?.name}
        open={isModalVisible}
        onCancel={closeModal}
        width={600}
        footer={[
          <Button key="close" onClick={closeModal}>
            Close
          </Button>,
          <Button key="calculate" onClick={calculateCost}>
            Calculate Price
          </Button>,
          <Button
            key="confirm"
            type="primary"
            loading={confirming}
            onClick={confirmPlanSelection}
          >
            Confirm Selection
          </Button>,
        ]}
      >
        {selectedPlan && (
          <>
            <Text strong>Enter your expected monthly consumption (kWh):</Text>
            <InputNumber
              min={0}
              value={consumption}
              onChange={(val) => setConsumption(val)}
              style={{ width: "100%", marginBottom: 12, marginTop: 4 }}
            />

            <Text strong>Select Tax Group:</Text>
            <Select
              placeholder="Select your tax group"
              style={{ width: "100%", marginBottom: 12, marginTop: 4 }}
              value={selectedTaxGroup?.id}
              onChange={(id) =>
                setSelectedTaxGroup(
                  taxGroups.find((tg) => tg.id === id)
                )
              }
            >
              {taxGroups.map((tg) => (
                <Option key={tg.id} value={tg.id}>
                  {tg.name} ({tg.vat}% VAT, {tg.ecoTax}% Eco)
                </Option>
              ))}
            </Select>

            {calculatedCost && (
              <Card
                style={{
                  marginTop: 16,
                  borderRadius: 12,
                  background: "#fff7e6",
                }}
                bodyStyle={{ padding: 16 }}
              >
                <Title level={5} style={{ color: "#fa8c16" }}>
                  Cost Breakdown
                </Title>
                <Divider />

                <Text>
                  Base cost: €{calculatedCost.baseCost.toFixed(2)}
                </Text>
                <br />

                <Text>
                  Discount: -€{calculatedCost.discountAmount.toFixed(2)}
                </Text>
                <br />

                <Text>
                  After discount: €
                  {calculatedCost.costAfterDiscount.toFixed(2)}
                </Text>
                <br />

                <Text>
                  VAT ({selectedTaxGroup.vat ?? 0}%): €
                  {calculatedCost.vatAmount.toFixed(2)}
                </Text>
                <br />

                <Text>
                  Eco Tax: €
                  {calculatedCost.ecoAmount.toFixed(2)}
                </Text>

                <Divider />

                <Text strong>
                  Total: €{calculatedCost.total.toFixed(2)}
                </Text>
              </Card>
            )}
          </>
        )}
      </Modal>

      {/* RECOMMENDATION MODAL */}
      <Modal
        title="Find Best Plan"
        open={isRecommendationModalVisible}
        onCancel={() => setIsRecommendationModalVisible(false)}
        footer={[
          <Button
            key="close"
            onClick={() => setIsRecommendationModalVisible(false)}
          >
            Cancel
          </Button>,
          <Button
            key="recommend"
            type="primary"
            loading={loadingRecommendation}
            onClick={getRecommendation}
          >
            Get Recommendation
          </Button>,
        ]}
      >
        <Text strong>Enter monthly consumption (kWh):</Text>
        <InputNumber
          min={0}
          style={{ width: "100%", marginTop: 6, marginBottom: 16 }}
          value={recConsumption}
          onChange={(val) => setRecConsumption(val)}
        />

        <Text strong>Select Tax Group:</Text>
        <Select
          placeholder="Select tax group"
          style={{ width: "100%", marginTop: 6 }}
          value={recTaxGroup?.id}
          onChange={(id) =>
            setRecTaxGroup(taxGroups.find((tg) => tg.id === id))
          }
        >
          {taxGroups.map((tg) => (
            <Option key={tg.id} value={tg.id}>
              {tg.name} ({tg.vat}% VAT, {tg.ecoTax}% Eco)
            </Option>
          ))}
        </Select>
      </Modal>
    </div>
  );
};

export default Home;
