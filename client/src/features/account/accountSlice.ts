import {User} from "../../app/models/user";
import {createAsyncThunk, createSlice, isAnyOf} from "@reduxjs/toolkit";
import {FieldValues} from "react-hook-form";
import agent from "../../app/api/agent";

interface AccountState {
    user: User | null
}
const initialState: AccountState = {
    user: null
}

export const sighInUser = createAsyncThunk<User, FieldValues>(
    'account/signInUser',
    async (data, thunkApi) => {
        try {
            const user = await agent.Account.login(data);
            localStorage.setItem('user', JSON.stringify(user));
            return user;
        } catch (error: any) {
            return thunkApi.rejectWithValue(({error: error.data}))
        }
    }
)

export const fetchCurrentUser = createAsyncThunk<User>(
    'account/signInUser',
    async (_, thunkApi) => {
        try {
           const user = await agent.Account.currentUser();
            localStorage.setItem('user', JSON.stringify(user));
            return user;
        } catch (error: any) {
            return thunkApi.rejectWithValue(({error: error.data}))
        }
    }
)
export const accountSlice = createSlice({
    name: 'account',
    initialState,
    reducers: {},
    extraReducers: (builder => {
        builder.addMatcher(isAnyOf(sighInUser.fulfilled,fetchCurrentUser.fulfilled),(state,action) => {
            state.user = action.payload
        });
        builder.addMatcher(isAnyOf(sighInUser.rejected,fetchCurrentUser.rejected),() => {
        });
    })
})
