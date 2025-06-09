import React from "react";
import Header from "../components/Header";
import "./Login.css";
import { Link } from "react-router-dom";
function Login() {
  return (
    <div>
      <Header />
      <div className="login">
        <h1>Welcome to EduLawAI!</h1>
        <div className="login content">
          <div className="login email">
            <input style={{}} type="text" placeholder="Enter your email" />
          </div>
          <div className="login password">
            <input type="text" placeholder="Enter your password" />
          </div>
          <div className="login button">
            <button>Login</button>
          </div>
        </div>
      </div>
      <div className="signup">
        <span>Don't have an account? </span>
        <Link to="/signup" className="signupText">
          Sign Up
        </Link>
      </div>
      <div className="login google">
        <span style={{ margin: "10px" }}>Or</span>
        <button className="googleButton">
          <img
            src="https://upload.wikimedia.org/wikipedia/commons/thumb/c/c1/Google_%22G%22_logo.svg/800px-Google_%22G%22_logo.svg.png"
            alt="Google Logo"
          />
          Login with Google
        </button>
      </div>
    </div>
  );
}

export default Login;
